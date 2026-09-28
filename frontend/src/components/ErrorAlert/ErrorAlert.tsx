export interface ErrorAlertProps {
  message: string
  onRetry?: () => void
  onReset?: () => void
  inline?: boolean
}

export function ErrorAlert({ message, onRetry, onReset, inline = false }: ErrorAlertProps) {
  if (inline) {
    return (
      <div className="error-banner" role="alert">
        <span>⚠️</span>
        <div>
          <strong>İşlem Uyarısı:</strong>
          <p>{message}</p>
        </div>
      </div>
    )
  }

  return (
    <section className="failed-view" role="alert">
      <div className="failed-icon">❌</div>
      <h2>E-Kitap Üretimi Başarısız Oldu</h2>
      <p className="failed-message">
        {message || 'Belgeler işlenirken beklenmeyen bir hata oluştu.'}
      </p>

      <div className="failed-actions">
        {onRetry && (
          <button type="button" className="btn-primary" onClick={onRetry}>
            🔄 Yeniden Dene
          </button>
        )}
        {onReset && (
          <button type="button" className="btn-secondary" onClick={onReset}>
            Yeni Kitap Başlat
          </button>
        )}
      </div>
    </section>
  )
}

export default ErrorAlert
