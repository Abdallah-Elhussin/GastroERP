/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Plus Jakarta Sans', 'Inter', 'system-ui', 'sans-serif'],
        arabic: ['Tajawal', 'Noto Sans Arabic', 'system-ui', 'sans-serif']
      }
    },
  },
  plugins: [],
}

