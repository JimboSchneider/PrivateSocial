interface WeatherForecast {
  date: string
  temperatureC: number
  temperatureF: number
  summary?: string
}

export async function getWeatherForecast(): Promise<WeatherForecast[]> {
  const response = await fetch('/api/weatherforecast')
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`)
  }
  
  const data = await response.json()
  return data
}

export { type WeatherForecast }