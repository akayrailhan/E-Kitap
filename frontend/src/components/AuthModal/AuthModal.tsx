import { useState } from 'react'
import { supabase } from '../../services/supabaseClient'

interface AuthModalProps {
  isOpen: boolean
  onClose: () => void
  onSuccess: (email: string) => void
}

export function AuthModal({ isOpen, onClose, onSuccess }: AuthModalProps) {
  const [isSignUp, setIsSignUp] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [info, setInfo] = useState<string | null>(null)

  if (!isOpen) return null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!supabase) {
      setError('Supabase bağlantısı henüz yapılandırılmamış.')
      return
    }

    setLoading(true)
    setError(null)
    setInfo(null)

    try {
      if (isSignUp) {
        const { data, error: signUpError } = await supabase.auth.signUp({
          email: email.trim(),
          password,
        })
        if (signUpError) throw signUpError
        if (data.session) {
          onSuccess(data.user?.email || email)
          onClose()
        } else {
          setInfo('Kayıt başarılı! Lütfen e-postanızı kontrol edin (veya doğrudan giriş yapmayı deneyin).')
        }
      } else {
        const { data, error: signInError } = await supabase.auth.signInWithPassword({
          email: email.trim(),
          password,
        })
        if (signInError) throw signInError
        onSuccess(data.user?.email || email)
        onClose()
      }
    } catch (err: unknown) {
      let message = err instanceof Error ? err.message : 'Kimlik doğrulama işlemi başarısız.'
      if (message.toLowerCase().includes('rate limit')) {
        message = 'Supabase e-posta limiti aşıldı. Lütfen Supabase panelinden "Confirm email" ayarını kapatın veya panelden doğrudan kullanıcı ekleyin.'
      } else if (message.toLowerCase().includes('invalid login credentials')) {
        message = 'E-posta veya şifre hatalı (ya da e-posta onayınız henüz tamamlanmamış).'
      }
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{isSignUp ? 'Yeni Hesap Oluştur' : 'Supabase ile Giriş Yap'}</h3>
          <button type="button" className="btn-modal-close" onClick={onClose} aria-label="Kapat">
            ✕
          </button>
        </div>

        <p className="modal-subtitle">
          Giriş yaptığınızda ürettiğiniz kitaplar Supabase kullanıcı kimliğinizle ilişkilendirilir.
        </p>

        <div className="demo-credentials-box">
          <div className="demo-text">
            <span>💡 <strong>Hızlı Test İçin:</strong></span>
            <div className="demo-creds">
              <code>test@example.com</code> / <code>123456</code>
            </div>
          </div>
          <button
            type="button"
            className="btn-demo-fill"
            onClick={() => {
              setEmail('test@example.com')
              setPassword('123456')
              setIsSignUp(false)
              setError(null)
              setInfo(null)
            }}
          >
            Bilgileri Doldur
          </button>
        </div>

        {error && <div className="modal-alert modal-error">{error}</div>}
        {info && <div className="modal-alert modal-info">{info}</div>}

        <form onSubmit={handleSubmit} className="modal-form">
          <label htmlFor="auth-email">E-Posta Adresi</label>
          <input
            id="auth-email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ornek@alanadi.com"
          />

          <label htmlFor="auth-password">Şifre</label>
          <input
            id="auth-password"
            type="password"
            required
            minLength={6}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="En az 6 karakter"
          />

          <button type="submit" className="btn-primary btn-modal-submit" disabled={loading}>
            {loading ? 'İşleniyor...' : isSignUp ? 'Kayıt Ol' : 'Giriş Yap'}
          </button>
        </form>

        <div className="modal-footer">
          {isSignUp ? (
            <p>
              Zaten bir hesabınız var mı?{' '}
              <button
                type="button"
                className="btn-link"
                onClick={() => {
                  setIsSignUp(false)
                  setError(null)
                  setInfo(null)
                }}
              >
                Giriş Yap
              </button>
            </p>
          ) : (
            <p>
              Hesabınız yok mu?{' '}
              <button
                type="button"
                className="btn-link"
                onClick={() => {
                  setIsSignUp(true)
                  setError(null)
                  setInfo(null)
                }}
              >
                Yeni Hesap Oluştur
              </button>
            </p>
          )}
        </div>
      </div>
    </div>
  )
}

export default AuthModal
