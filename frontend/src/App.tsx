import { useEffect, useRef, useState } from 'react'
import './App.css'
import {
  BookForm,
  ErrorAlert,
  FileList,
  Header,
  LoadingProgress,
  PdfViewer,
} from './components'
import { createBookPdf, getBookStatus, uploadBook } from './services/bookApi'
import type { AppStep, BookStatusData } from './types/book'

function App() {
  const [bookName, setBookName] = useState('')
  const [files, setFiles] = useState<File[]>([])
  const [appStep, setAppStep] = useState<AppStep>('draft')
  const [currentBookId, setCurrentBookId] = useState<string | null>(null)
  const [bookData, setBookData] = useState<BookStatusData | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [progressPercent, setProgressPercent] = useState<number>(0)
  const [stageIndex, setStageIndex] = useState<number>(0)

  const pollIntervalRef = useRef<number | null>(null)

  useEffect(() => {
    return () => {
      if (pollIntervalRef.current) {
        clearInterval(pollIntervalRef.current)
      }
    }
  }, [])

  const handleFilesSelected = (selectedFiles: FileList | null) => {
    if (!selectedFiles || selectedFiles.length === 0) return
    setError(null)

    const incoming = Array.from(selectedFiles)
    const invalid = incoming.filter((f) => !f.name.toLowerCase().endsWith('.docx'))
    if (invalid.length > 0) {
      setError(`Yalnızca .docx dosyaları kabul edilir. Geçersiz dosya: ${invalid.map((f) => f.name).join(', ')}`)
      return
    }

    setFiles((prev) => {
      if (incoming.length === 10) {
        return incoming
      }
      const combined = [...prev, ...incoming]
      if (combined.length > 10) {
        setError('En fazla 10 bildiri eklenebilir. İlk 10 dosya alındı.')
        return combined.slice(0, 10)
      }
      return combined
    })
  }

  const handleRemoveFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index))
    setError(null)
  }

  const handleMoveFile = (index: number, direction: -1 | 1) => {
    setFiles((prev) => {
      const nextIndex = index + direction
      if (nextIndex < 0 || nextIndex >= prev.length) return prev
      const updated = [...prev]
      const [moved] = updated.splice(index, 1)
      updated.splice(nextIndex, 0, moved)
      return updated
    })
  }

  const handleClearFiles = () => {
    setFiles([])
    setError(null)
  }

  const isFormValid = bookName.trim().length > 0 && files.length === 10
  const isSubmitting = appStep === 'uploading' || appStep === 'processing'

  const startPolling = (bookId: string) => {
    if (pollIntervalRef.current) {
      clearInterval(pollIntervalRef.current)
    }

    let simulatedPercent = 30
    pollIntervalRef.current = window.setInterval(async () => {
      try {
        const data = await getBookStatus(bookId)
        setBookData(data)

        simulatedPercent = Math.min(simulatedPercent + 12, 92)
        setProgressPercent(simulatedPercent)

        if (simulatedPercent < 45) setStageIndex(1)
        else if (simulatedPercent < 70) setStageIndex(2)
        else if (simulatedPercent < 88) setStageIndex(3)
        else setStageIndex(4)

        const status = data.status
        const isCompleted = status === 'Completed' || status === 2
        const isFailed = status === 'Failed' || status === 3

        if (isCompleted) {
          if (pollIntervalRef.current) clearInterval(pollIntervalRef.current)
          setProgressPercent(100)
          setStageIndex(4)
          setTimeout(() => {
            setAppStep('completed')
          }, 600)
        } else if (isFailed) {
          if (pollIntervalRef.current) clearInterval(pollIntervalRef.current)
          setError(data.errorMessage || 'Kitap üretimi sırasında bir hata oluştu.')
          setAppStep('failed')
        }
      } catch (err: unknown) {
        console.error('Polling error:', err)
      }
    }, 1200)
  }

  const handleCreateBook = async () => {
    if (!isFormValid || isSubmitting) return

    setError(null)
    setAppStep('uploading')
    setProgressPercent(15)
    setStageIndex(0)

    try {
      const uploadResult = await uploadBook(bookName, files)
      const bookId = uploadResult.bookId
      setCurrentBookId(bookId)

      setAppStep('processing')
      setProgressPercent(30)
      setStageIndex(1)

      await createBookPdf(bookId)
      startPolling(bookId)
    } catch (err: unknown) {
      console.error(err)
      const message = err instanceof Error ? err.message : 'Beklenmeyen bir hata oluştu.'
      setError(message)
      setAppStep('draft')
    }
  }

  const handleRetry = async () => {
    if (!currentBookId) return
    setError(null)
    setAppStep('processing')
    setProgressPercent(25)
    setStageIndex(0)

    try {
      await createBookPdf(currentBookId)
      startPolling(currentBookId)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Yeniden başlatma başarısız.'
      setError(message)
      setAppStep('failed')
    }
  }

  const handleNewBook = () => {
    if (pollIntervalRef.current) clearInterval(pollIntervalRef.current)
    setBookName('')
    setFiles([])
    setCurrentBookId(null)
    setBookData(null)
    setError(null)
    setAppStep('draft')
    setProgressPercent(0)
    setStageIndex(0)
  }

  return (
    <main className="workspace">
      <Header appStep={appStep} />

      {/* DRAFT STATE: BOOK FORM & FILE LIST */}
      {appStep === 'draft' && (
        <>
          <section className="intro">
            <p className="eyebrow">YENİ KİTAP</p>
            <h2>On bildiriyi tek bir yayına dönüştürün.</h2>
            <p className="intro-copy">
              Kitap adını belirleyin, tam 10 adet Word (.docx) bildirisini ekleyin. İletişim bilgileri
              (e-posta, telefon) otomatik temizlenir; içindekiler tablosu ve sayfa numaralarıyla tek PDF oluşturulur.
            </p>
          </section>

          {error && <ErrorAlert message={error} inline />}

          <section className="editor-grid" aria-label="Kitap oluşturma alanı">
            <BookForm
              bookName={bookName}
              onBookNameChange={setBookName}
              filesCount={files.length}
              onFilesSelected={handleFilesSelected}
            />

            <FileList
              files={files}
              onRemove={handleRemoveFile}
              onMove={handleMoveFile}
              onClear={handleClearFiles}
            />
          </section>

          <footer className="workspace-footer">
            <div className="footer-status-text">
              {!bookName.trim() && files.length < 10 && (
                <span>Lütfen kitap adını girin ve 10 adet bildiri seçin.</span>
              )}
              {!bookName.trim() && files.length === 10 && (
                <span>Lütfen kitap adını belirleyin.</span>
              )}
              {bookName.trim() && files.length < 10 && (
                <span>{10 - files.length} bildiri daha eklenmesi gerekiyor.</span>
              )}
              {isFormValid && (
                <span className="ready-text">✓ Hazır! PDF üretimine başlayabilirsiniz.</span>
              )}
            </div>

            <button
              type="button"
              className="btn-primary"
              disabled={!isFormValid || isSubmitting}
              onClick={handleCreateBook}
            >
              Kitabı Oluştur
            </button>
          </footer>
        </>
      )}

      {/* LOADING & PROCESSING STATE */}
      {(appStep === 'uploading' || appStep === 'processing') && (
        <LoadingProgress
          bookName={bookName}
          isUploading={appStep === 'uploading'}
          progressPercent={progressPercent}
          stageIndex={stageIndex}
        />
      )}

      {/* COMPLETED STATE: PDF VIEWER & ACTIONS */}
      {appStep === 'completed' && currentBookId && (
        <PdfViewer
          bookId={currentBookId}
          bookData={bookData}
          bookName={bookName}
          onNewBook={handleNewBook}
        />
      )}

      {/* FAILED STATE: ERROR VIEW & RETRY */}
      {appStep === 'failed' && (
        <ErrorAlert
          message={error || 'Kitap üretimi tamamlanamadı.'}
          onRetry={handleRetry}
          onReset={handleNewBook}
        />
      )}
    </main>
  )
}

export default App
