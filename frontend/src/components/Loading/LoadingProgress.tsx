import { LOADING_STAGES } from '../../types/book'

export interface LoadingProgressProps {
  bookName: string
  isUploading: boolean
  progressPercent: number
  stageIndex: number
}

export function LoadingProgress({
  bookName,
  isUploading,
  progressPercent,
  stageIndex,
}: LoadingProgressProps) {
  return (
    <section className="loading-card" aria-live="polite">
      <div className="loading-header">
        <div className="loading-spinner" aria-hidden="true" />
        <div>
          <p className="eyebrow">
            {isUploading ? 'DOSYALAR YÜKLENİYOR' : 'E-KİTAP DERLENİYOR'}
          </p>
          <h2>{bookName}</h2>
          <p className="loading-sub">
            10 bildiri taranıyor, iletişim bilgileri temizleniyor ve tek PDF üretiliyor...
          </p>
        </div>
      </div>

      <div className="progress-container">
        <div className="progress-bar-track">
          <div
            className="progress-bar-fill"
            style={{ width: `${progressPercent}%` }}
          />
        </div>
        <div className="progress-meta">
          <span>{LOADING_STAGES[stageIndex]?.label || 'İşleniyor...'}</span>
          <strong>%{progressPercent}</strong>
        </div>
      </div>

      <div className="stages-list">
        {LOADING_STAGES.map((st, i) => (
          <div
            key={st.label}
            className={`stage-item ${i < stageIndex ? 'done' : ''} ${i === stageIndex ? 'active' : ''}`}
          >
            <div className="stage-bullet">
              {i < stageIndex ? '✓' : i + 1}
            </div>
            <span>{st.label}</span>
          </div>
        ))}
      </div>

      <div className="loading-hint">
        💡 <em>Bu işlem bildirilerin sayfa uzunluğuna göre birkaç saniye sürebilir. Lütfen sayfayı kapatmayınız.</em>
      </div>
    </section>
  )
}

export default LoadingProgress
