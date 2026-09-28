import { useEffect, useState } from 'react'
import { getMyBooks, getPdfDownloadUrl } from '../../services/bookApi'
import type { BookListItem } from '../../types/book'

interface MyBooksModalProps {
  isOpen: boolean
  onClose: () => void
  onSelectBook: (bookId: string) => void
}

export function MyBooksModal({ isOpen, onClose, onSelectBook }: MyBooksModalProps) {
  const [books, setBooks] = useState<BookListItem[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchBooks = async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getMyBooks()
      setBooks(data)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Kitaplar yüklenirken bir hata oluştu.'
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (isOpen) {
      fetchBooks()
    }
  }, [isOpen])

  if (!isOpen) return null

  const getStatusBadge = (status: BookListItem['status']) => {
    const isCompleted = status === 'Completed' || status === 2
    const isProcessing = status === 'Processing' || status === 1
    const isFailed = status === 'Failed' || status === 3

    if (isCompleted) return <span className="status-badge status-completed">Tamamlandı</span>
    if (isProcessing) return <span className="status-badge status-processing">Üretiliyor</span>
    if (isFailed) return <span className="status-badge status-failed">Hata</span>
    return <span className="status-badge status-draft">Taslak</span>
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card modal-books-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div>
            <p className="eyebrow" style={{ margin: 0 }}>GEÇMİŞ YAYINLAR</p>
            <h3>Oluşturulan Kitaplarım</h3>
          </div>
          <button type="button" className="btn-modal-close" onClick={onClose} aria-label="Kapat">
            ✕
          </button>
        </div>

        <p className="modal-subtitle">
          Hesabınızla ürettiğiniz tüm e-kitapları burada görebilir, tek tıkla görüntüleyebilir veya indirebilirsiniz.
        </p>

        {loading && (
          <div className="books-loading">
            <div className="loading-spinner" style={{ width: 24, height: 24 }} />
            <span>Kitaplar yükleniyor...</span>
          </div>
        )}

        {error && <div className="modal-alert modal-error">{error}</div>}

        {!loading && books.length === 0 && !error && (
          <div className="empty-books-state">
            <span style={{ fontSize: 32 }}>📚</span>
            <p><strong>Henüz kayıtlı bir kitap bulunamadı.</strong></p>
            <small>Yeni bir kitap oluşturduğunuzda burada listelenecektir.</small>
          </div>
        )}

        {!loading && books.length > 0 && (
          <div className="my-books-list">
            {books.map((book) => {
              const isCompleted = book.status === 'Completed' || book.status === 2
              const formattedDate = new Date(book.createdAt).toLocaleDateString('tr-TR', {
                day: 'numeric',
                month: 'short',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
              })

              return (
                <div key={book.bookId} className="my-book-item">
                  <div className="my-book-info">
                    <div className="my-book-meta">
                      {getStatusBadge(book.status)}
                      <span className="my-book-date">{formattedDate}</span>
                      <span className="my-book-count">· {book.paperCount} Bildiri</span>
                    </div>
                    <h4 className="my-book-title">{book.name}</h4>
                    {book.errorMessage && (
                      <p className="my-book-error">{book.errorMessage}</p>
                    )}
                  </div>

                  <div className="my-book-actions">
                    {isCompleted ? (
                      <>
                        <button
                          type="button"
                          className="btn-book-action btn-book-view"
                          onClick={() => {
                            onSelectBook(book.bookId)
                            onClose()
                          }}
                        >
                          👁️ Görüntüle
                        </button>
                        <a
                          href={getPdfDownloadUrl(book.bookId)}
                          download={`${book.name}.pdf`}
                          className="btn-book-action btn-book-dl"
                          title="PDF olarak indir"
                        >
                          📥 İndir
                        </a>
                      </>
                    ) : (
                      <button
                        type="button"
                        className="btn-book-action btn-book-view"
                        onClick={() => {
                          onSelectBook(book.bookId)
                          onClose()
                        }}
                      >
                        Durumu Gör
                      </button>
                    )}
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}

export default MyBooksModal
