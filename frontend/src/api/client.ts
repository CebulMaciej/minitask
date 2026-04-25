import axios from 'axios'

export const apiClient = axios.create({
  baseURL: '/api',
  withCredentials: true
})

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

let isRefreshing = false
let failedQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = []

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach((p) => (error ? p.reject(error) : p.resolve(token!)))
  failedQueue = []
}

apiClient.interceptors.response.use(
  (res) => res,
  async (error) => {
    const original = error.config

    if (error.response?.status === 401 && !original._retry && original.url !== '/auth/refresh') {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            original.headers.Authorization = `Bearer ${token}`
            return apiClient(original)
          })
          .catch(Promise.reject.bind(Promise))
      }

      original._retry = true
      isRefreshing = true

      try {
        const res = await apiClient.post('/auth/refresh')
        const newToken = res.data.accessToken
        setAccessToken(newToken)
        processQueue(null, newToken)
        original.headers.Authorization = `Bearer ${newToken}`
        return apiClient(original)
      } catch (refreshError) {
        processQueue(refreshError, null)
        setAccessToken(null)
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    if (error.response?.status !== 401) {
      const msg =
        error.response?.data?.message ?? error.response?.data?.title ?? 'Something went wrong'
      _errorHandler?.(msg)
    }

    return Promise.reject(error)
  }
)

let _errorHandler: ((msg: string) => void) | null = null

export function setErrorHandler(fn: (msg: string) => void) {
  _errorHandler = fn
}

let _accessToken: string | null = null

function getAccessToken() {
  return _accessToken
}

function setAccessToken(token: string | null) {
  _accessToken = token
}

export { setAccessToken, getAccessToken }
