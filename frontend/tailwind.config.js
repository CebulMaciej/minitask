/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: '#C8FF00',
        'primary-dark': '#A3D400',
        accent: '#FF6B35',
        'bg-base': '#0D0D0D',
        'bg-surface': '#1A1A1A',
        'bg-elevated': '#242424',
        'bg-subtle': '#2E2E2E',
        'text-primary': '#F5F5F5',
        'text-secondary': '#A0A0A0',
        'text-muted': '#606060',
        'text-inverse': '#0D0D0D',
        'border-default': '#2E2E2E',
        'border-subtle': '#1F1F1F'
      },
      fontFamily: {
        display: ['"Bebas Neue"', '"Impact"', 'sans-serif'],
        body: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'monospace']
      },
      boxShadow: {
        'glow-primary': '0 0 12px rgba(200, 255, 0, 0.25)',
        card: '0 1px 3px rgba(0, 0, 0, 0.5)',
        elevated: '0 8px 24px rgba(0, 0, 0, 0.6)'
      }
    }
  },
  plugins: []
}
