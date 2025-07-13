interface ErrorFallbackProps {
  resetErrorBoundary: () => void
}

function ErrorFallback({ resetErrorBoundary }: ErrorFallbackProps) {
  return (
    <div id="blazor-error-ui">
      An unhandled error has occurred.
      <a href="" className="reload" onClick={(e) => { e.preventDefault(); resetErrorBoundary() }}>Reload</a>
      <a className="dismiss">🗙</a>
    </div>
  )
}

export default ErrorFallback