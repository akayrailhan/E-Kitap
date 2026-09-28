import { getPdfDownloadUrl, getPdfUrl } from '../../services/bookApi'
import type { BookStatusData } from '../../types/book'

export interface PdfViewerProps {
  bookId: string
  bookData: BookStatusData | null
  bookName: string
  onNewBook: () => void
}

export function PdfViewer({ bookId, bookData, bookName, onNewBook }: PdfViewerProps) {
  const title = bookData?.name || bookName
  const pdfUrl = getPdfUrl(bookId)
  const downloadUrl = getPdfDownloadUrl(bookId)
  const papers = bookData?.papers || []

  return (
    <section className="completed-view">
      <div className="completed-header">
        <div>
          <p className="eyebrow">YAYIN HAZIR</p>
          <h2>{title}</h2>
          <p className="intro-copy">
            10 bildiri başarıyla derlendi. İletişim bilgileri arındırıldı, içindekiler tablosu ve sayfa numaraları oluşturuldu.
          </p>
        </div>

        <div className="completed-actions">
          <a
            href={downloadUrl}
            download={`${title}.pdf`}
            className="btn-download"
          >
            📥 PDF İndir
          </a>
          <a
            href={pdfUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="btn-secondary"
          >
            ↗ Yeni Sekmede Aç
          </a>
          <button type="button" onClick={onNewBook} className="btn-secondary">
            + Yeni Kitap
          </button>
        </div>
      </div>

      <div className="viewer-grid">
        <div className="pdf-frame-container">
          <iframe
            src={pdfUrl}
            title="E-Kitap PDF Görüntüleyici"
            className="pdf-iframe"
          />
        </div>

        <aside className="toc-sidebar">
          <h3>İçindekiler</h3>
          <p className="toc-desc">Üretilen e-kitap içindeki bildiriler:</p>
          <ol className="toc-list">
            {papers.map((paper) => (
              <li key={paper.id}>
                <span className="toc-num">{String(paper.order).padStart(2, '0')}</span>
                <div className="toc-content">
                  <b>{paper.title || paper.originalFileName}</b>
                  <small>{paper.originalFileName}</small>
                </div>
              </li>
            ))}
          </ol>
        </aside>
      </div>
    </section>
  )
}

export default PdfViewer
