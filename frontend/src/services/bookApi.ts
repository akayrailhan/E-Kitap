import type { BookStatusData, BookUploadResponse } from '../types/book'

export async function uploadBook(bookName: string, files: File[]): Promise<BookUploadResponse> {
  const formData = new FormData()
  formData.append('bookName', bookName.trim())
  files.forEach((file) => {
    formData.append('papers', file)
  })

  const response = await fetch('/api/books', {
    method: 'POST',
    body: formData,
  })

  if (!response.ok) {
    if (response.status === 502) {
      throw new Error("Backend API sunucusuna ulaşılamadı (502 Bad Gateway). Lütfen backend'in (http://localhost:5290) çalıştığından emin olun.")
    }
    const errorData = await response.json().catch(() => ({}))
    throw new Error(errorData.message || `Yükleme işlemi başarısız oldu (${response.status})`)
  }

  return response.json()
}

export async function createBookPdf(bookId: string): Promise<BookStatusData> {
  const response = await fetch(`/api/books/${bookId}/create`, {
    method: 'POST',
  })

  if (!response.ok) {
    if (response.status === 502) {
      throw new Error("Backend API sunucusuna ulaşılamadı (502 Bad Gateway).")
    }
    const errorData = await response.json().catch(() => ({}))
    throw new Error(errorData.message || `E-Kitap üretimi başlatılamadı (${response.status})`)
  }

  return response.json()
}

export async function getBookStatus(bookId: string): Promise<BookStatusData> {
  const response = await fetch(`/api/books/${bookId}`)

  if (!response.ok) {
    if (response.status === 502) {
      throw new Error("Backend API sunucusuna ulaşılamadı (502 Bad Gateway).")
    }
    const errorData = await response.json().catch(() => ({}))
    throw new Error(errorData.message || `Kitap durumu sorgulanamadı (${response.status})`)
  }

  return response.json()
}

export function getPdfUrl(bookId: string): string {
  return `/api/books/${bookId}/pdf`
}

export function getPdfDownloadUrl(bookId: string): string {
  return `/api/books/${bookId}/pdf?download=true`
}

