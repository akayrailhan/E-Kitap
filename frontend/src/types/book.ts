export type BookStatus = 'Draft' | 'Processing' | 'Completed' | 'Failed' | number

export interface PaperItem {
  id: string
  originalFileName: string
  title?: string | null
  order: number
}

export interface BookStatusData {
  id: string
  name: string
  status: BookStatus
  errorMessage?: string | null
  pdfPath?: string | null
  papers: PaperItem[]
}

export interface BookUploadResponse {
  bookId: string
  name: string
  papers: {
    id: string
    originalFileName: string
    order: number
  }[]
}

export interface BookListItem {
  bookId: string
  name: string
  status: BookStatus
  createdAt: string
  completedAt?: string | null
  pdfPath?: string | null
  paperCount: number
  errorMessage?: string | null
}

export type AppStep = 'draft' | 'uploading' | 'processing' | 'completed' | 'failed'

export interface LoadingStage {
  label: string
  percent: number
}

export const LOADING_STAGES: LoadingStage[] = [
  { label: 'Belgeler sunucuya aktarılıyor', percent: 20 },
  { label: 'DOCX bildiri metinleri ayrıştırılıyor', percent: 45 },
  { label: 'E-posta ve telefon bilgileri temizleniyor', percent: 70 },
  { label: 'İçindekiler tablosu ve sayfa numaraları hazırlanıyor', percent: 85 },
  { label: 'Tek PDF e-kitap derleniyor ve mizanpaj tamamlanıyor', percent: 98 },
]
