import { useState } from 'react'

function Counter() {
  const [currentCount, setCurrentCount] = useState(0)

  const incrementCount = () => {
    setCurrentCount(currentCount + 1)
  }

  return (
    <>
      <h1>Counter</h1>
      <p role="status">Current count: {currentCount}</p>
      <button className="btn btn-primary" onClick={incrementCount}>Click me</button>
    </>
  )
}

export default Counter