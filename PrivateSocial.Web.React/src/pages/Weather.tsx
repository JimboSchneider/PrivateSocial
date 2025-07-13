import { useState, useEffect } from 'react'
import { getWeatherForecast } from '../services/weatherService'

interface WeatherForecast {
  date: string
  temperatureC: number
  temperatureF: number
  summary?: string
}

function Weather() {
  const [forecasts, setForecasts] = useState<WeatherForecast[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadWeatherData()
  }, [])

  const loadWeatherData = async () => {
    try {
      setLoading(true)
      const data = await getWeatherForecast()
      setForecasts(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load weather data')
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return <p><em>Loading...</em></p>
  }

  if (error) {
    return <p className="text-danger">Error: {error}</p>
  }

  return (
    <>
      <h1>Weather</h1>
      <p>This component demonstrates fetching data from the server.</p>
      
      {forecasts && forecasts.length > 0 ? (
        <table className="table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Temp. (C)</th>
              <th>Temp. (F)</th>
              <th>Summary</th>
            </tr>
          </thead>
          <tbody>
            {forecasts.map((forecast, index) => (
              <tr key={index}>
                <td>{new Date(forecast.date).toLocaleDateString()}</td>
                <td>{forecast.temperatureC}</td>
                <td>{forecast.temperatureF}</td>
                <td>{forecast.summary}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p>No weather data available.</p>
      )}
    </>
  )
}

export default Weather