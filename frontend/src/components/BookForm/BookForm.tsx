import { useRef, useState } from 'react'

export interface BookFormProps {
  bookName: string
  onBookNameChange: (value: string) => void
  filesCount: number
  onFilesSelected: (files: FileList | null) => void
}

export function BookForm({
  bookName,
  onBookNameChange,
  filesCount,
  onFilesSelected,
}: BookFormProps) {
  const [isDragging, setIsDragging] = useState(false)
  const fileInputRef = useRef<HTMLInputElement | null>(null)

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault()
    setIsDragging(true)
  }

  const handleDragLeave = () => {
    setIsDragging(false)
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setIsDragging(false)
    onFilesSelected(e.dataTransfer.files)
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onFilesSelected(e.target.files)
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  return (
    <div className="form-panel">
      <label htmlFor="book-name">
        Kitap Adı <span className="required">*</span>
      </label>
      <input
        id="book-name"
        type="text"
        value={bookName}
        onChange={(e) => onBookNameChange(e.target.value)}
        placeholder="Örn. 2026 Uluslararası Bilişim Kongresi Bildirileri"
        maxLength={200}
      />

      <div
        className={`upload-dropzone ${isDragging ? 'dragging' : ''}`}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
      >
        <label className="upload-label" htmlFor="papers">
          <span className="upload-icon" aria-hidden="true">+</span>
          <span>
            <strong>Word bildirilerini seçin veya sürükleyin</strong>
            <small>Yalnızca .docx formatı · Tam olarak 10 dosya</small>
          </span>
        </label>
        <input
          ref={fileInputRef}
          id="papers"
          className="visually-hidden"
          type="file"
          accept=".docx,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
          multiple
          onChange={handleChange}
        />
      </div>

      <div className="requirements-box">
        <p className="req-title">Kabul Kriterleri & Kurallar:</p>
        <ul>
          <li className={bookName.trim() ? 'valid' : ''}>
            {bookName.trim() ? '✓' : '○'} Kitap adı belirlenmelidir.
          </li>
          <li className={filesCount === 10 ? 'valid' : ''}>
            {filesCount === 10 ? '✓' : '○'} Tam olarak 10 adet .docx bildirisi eklenmelidir ({filesCount}/10).
          </li>
          <li>○ E-posta ve telefon numaraları PDF çıktısında otomatik temizlenir.</li>
          <li>○ İçindekiler tablosu doğru sayfa numaralarıyla derlenir.</li>
        </ul>
      </div>
    </div>
  )
}

export default BookForm
