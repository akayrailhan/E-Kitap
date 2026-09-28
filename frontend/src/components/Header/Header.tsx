import { useEffect, useState } from 'react'
import { supabase } from '../../services/supabaseClient'
import type { AppStep } from '../../types/book'
import { AuthModal } from '../AuthModal'

export interface HeaderProps {
  appStep: AppStep
}

export function Header({ appStep }: HeaderProps) {
  const [userEmail, setUserEmail] = useState<string | null>(null)
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false)

  useEffect(() => {
    if (!supabase) return

    // Get initial session
    supabase.auth.getSession().then(({ data }) => {
      setUserEmail(data.session?.user?.email || null)
    })

    // Listen for auth changes
    const { data: authListener } = supabase.auth.onAuthStateChange((_event, session) => {
      setUserEmail(session?.user?.email || null)
    })

    return () => {
      authListener.subscription.unsubscribe()
    }
  }, [])

  const handleSignOut = async () => {
    if (!supabase) return
    await supabase.auth.signOut()
    setUserEmail(null)
  }

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
    <>
      <header className="topbar">
        <div className="brand-group">
          <div className="brand-mark" aria-hidden="true">
            E
          </div>
          <div>
            <p className="eyebrow">ETKİNLİK YAYIN AKIŞI</p>
            <h1>E-Kitap Oluşturucu</h1>
          </div>
        </div>

        <div className="topbar-actions">
          <span className={`status-badge status-${appStep}`}>{getBadgeLabel()}</span>

          <div className="auth-status">
            {userEmail ? (
              <div className="user-profile">
                <span className="user-avatar" title={userEmail}>
                  {userEmail.charAt(0).toUpperCase()}
                </span>
                <span className="user-email-text" title={userEmail}>
                  {userEmail}
                </span>
                <button
                  type="button"
                  className="btn-auth-action"
                  onClick={handleSignOut}
                  title="Çıkış yap"
                >
                  Çıkış
                </button>
              </div>
            ) : (
              <div className="guest-profile">
                <span className="guest-badge" title="Giriş yapmadan da tüm özellikleri kullanabilirsiniz">
                  Misafir
                </span>
                <button
                  type="button"
                  className="btn-auth-login"
                  onClick={() => setIsAuthModalOpen(true)}
                >
                  Giriş Yap / Kayıt Ol
                </button>
              </div>
            )}
          </div>
        </div>
      </header>

      <AuthModal
        isOpen={isAuthModalOpen}
        onClose={() => setIsAuthModalOpen(false)}
        onSuccess={(email) => setUserEmail(email)}
      />
    </>
  )
}

export default Header
