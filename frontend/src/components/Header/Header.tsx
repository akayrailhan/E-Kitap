import type { AppStep } from '../../types/book'

export interface HeaderProps {
  appStep: AppStep
}

export function Header({ appStep }: HeaderProps) {
  const getBadgeLabel = () => {
    switch (appStep) {
      case 'draft':
        return 'Taslak'
      case 'uploading':
        return 'Yükleniyor'
      case 'processing':
        return 'Üretiliyor'
      case 'completed':
        return 'Tamamlandı'
      case 'failed':
        return 'Hata'
    }
  }

  return (
    <header className="topbar">
      <div className="brand-mark" aria-hidden="true">
        E
      </div>
      <div>
        <p className="eyebrow">ETKİNLİK YAYIN AKIŞI</p>
        <h1>E-Kitap Oluşturucu</h1>
      </div>
      <span className={`status-badge status-${appStep}`}>{getBadgeLabel()}</span>
    </header>
  )
}

export default Header
