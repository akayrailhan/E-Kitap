export interface FileListProps {
  files: File[]
  onRemove: (index: number) => void
  onMove: (index: number, direction: -1 | 1) => void
  onClear: () => void
}

export function FileList({ files, onRemove, onMove, onClear }: FileListProps) {
  return (
    <aside className="summary-panel">
      <div className="summary-heading">
        <div>
          <span>Bildiri Sırası</span>
          <small style={{ display: 'block', fontSize: '11px', color: 'var(--muted)' }}>
            Sayfa sıralamasını oklar ile değiştirebilirsiniz
          </small>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          {files.length > 0 && (
            <button
              type="button"
              className="btn-clear"
              onClick={onClear}
              title="Listeyi temizle"
            >
              Temizle
            </button>
          )}
          <strong className={files.length === 10 ? 'count-ready' : ''}>
            {files.length} / 10
          </strong>
        </div>
      </div>

      {files.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon" aria-hidden="true">📄</div>
          <p>Henüz dosya eklenmedi.</p>
          <small>Dosyalarınızı seçtiğinizde burada yükleme sırasıyla listelenecektir.</small>
        </div>
      ) : (
        <ol className="file-list">
          {files.map((file, index) => (
            <li key={`${file.name}-${file.size}-${index}`}>
              <span className="file-order">{String(index + 1).padStart(2, '0')}</span>
              <div className="file-info">
                <b title={file.name}>{file.name}</b>
                <small>{(file.size / 1024).toFixed(1)} KB</small>
              </div>
              <div className="file-actions">
                <button
                  type="button"
                  className="btn-icon"
                  disabled={index === 0}
                  onClick={() => onMove(index, -1)}
                  title="Yukarı taşı"
                  aria-label="Yukarı taşı"
                >
                  ▲
                </button>
                <button
                  type="button"
                  className="btn-icon"
                  disabled={index === files.length - 1}
                  onClick={() => onMove(index, 1)}
                  title="Aşağı taşı"
                  aria-label="Aşağı taşı"
                >
                  ▼
                </button>
                <button
                  type="button"
                  className="btn-icon btn-remove"
                  onClick={() => onRemove(index)}
                  title="Kaldır"
                  aria-label="Kaldır"
                >
                  ✕
                </button>
              </div>
            </li>
          ))}
        </ol>
      )}
    </aside>
  )
}

export default FileList
