import { useState } from 'react'
import './App.css'

function App() {
  const [bookName, setBookName] = useState('')
  const [files, setFiles] = useState<File[]>([])

  const handleFiles = (selectedFiles: FileList | null) => {
    if (!selectedFiles) {
      return
    }

    setFiles(Array.from(selectedFiles))
  }

  return (
    <main className="workspace">
      <header className="topbar">
        <div className="brand-mark" aria-hidden="true">E</div>
        <div>
          <p className="eyebrow">ETKİNLİK YAYIN AKIŞI</p>
          <h1>E-Kitap oluştur</h1>
        </div>
        <span className="draft-status">Taslak</span>
      </header>

      <section className="intro">
        <p className="eyebrow">YENİ KİTAP</p>
        <h2>On bildiriyi tek bir yayına dönüştür.</h2>
        <p className="intro-copy">
          Kitap adını belirle, bildirileri sıralı şekilde ekle. PDF üretimi başladığında içerik ve sayfa düzeni korunur.
        </p>
      </section>

      <section className="editor-grid" aria-label="Kitap oluşturma alanı">
        <div className="form-panel">
          <label htmlFor="book-name">Kitap adı</label>
          <input
            id="book-name"
            type="text"
            value={bookName}
            onChange={(event) => setBookName(event.target.value)}
            placeholder="Örn. 2026 Bilimsel Araştırmalar"
          />

          <label className="upload-label" htmlFor="papers">
            <span className="upload-icon" aria-hidden="true">+</span>
            <span>
              <strong>Word bildirilerini ekle</strong>
              <small>Yalnızca .docx · Tam 10 dosya</small>
            </span>
          </label>
          <input
            id="papers"
            className="visually-hidden"
            type="file"
            accept=".docx,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            multiple
            onChange={(event) => handleFiles(event.target.files)}
          />
        </div>

        <aside className="summary-panel">
          <div className="summary-heading">
            <span>Bildiri sırası</span>
            <strong>{files.length} / 10</strong>
          </div>
          {files.length === 0 ? (
            <p className="empty-state">Dosyalar eklendiğinde burada yükleme sırasıyla görünecek.</p>
          ) : (
            <ol className="file-list">
              {files.map((file, index) => (
                <li key={`${file.name}-${file.lastModified}`}>
                  <span>{String(index + 1).padStart(2, '0')}</span>
                  <b>{file.name}</b>
                </li>
              ))}
            </ol>
          )}
        </aside>
      </section>

      <footer className="workspace-footer">
        <span>PDF üretimi, 10 bildiri tamamlandığında kullanılabilir olacak.</span>
        <button type="button" disabled={!bookName.trim() || files.length !== 10}>
          Kitabı oluştur
        </button>
      </footer>
    </main>
  )
}

export default App
