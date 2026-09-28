import { createClient, type SupabaseClient } from '@supabase/supabase-js'

function getCleanEnvVar(key: string): string {
  const value = import.meta.env[key]
  if (!value || typeof value !== 'string') return ''
  // Strip quotes, angle brackets and whitespace
  return value.trim().replace(/^["']|["']$/g, '').replace(/[<>]/g, '')
}

function normalizeSupabaseUrl(key: string): string {
  let url = getCleanEnvVar(key)
  if (!url) return ''

  // If user provided just the project ID (e.g. "xyzabcdef"), format to full URL
  if (!url.startsWith('http://') && !url.startsWith('https://')) {
    url = `https://${url}.supabase.co`
  }

  // Ensure trailing slashes are removed
  return url.replace(/\/+$/, '')
}

function initSupabase(): { client: SupabaseClient | null; isConfigured: boolean } {
  try {
    const rawUrl = normalizeSupabaseUrl('VITE_SUPABASE_URL')
    const rawKey = getCleanEnvVar('VITE_SUPABASE_ANON_KEY')

    if (
      !rawUrl ||
      !rawKey ||
      rawUrl.includes('your-project') ||
      rawKey === 'your-anon-key'
    ) {
      return { client: null, isConfigured: false }
    }

    // Verify it parses as a valid URL
    const parsed = new URL(rawUrl)
    if (!parsed.hostname.includes('.')) {
      return { client: null, isConfigured: false }
    }

    const client = createClient(rawUrl, rawKey, {
      auth: {
        persistSession: true,
        autoRefreshToken: true,
      },
    })

    return { client, isConfigured: true }
  } catch (error) {
    console.warn('Supabase istemcisi başlatılamadı:', error)
    return { client: null, isConfigured: false }
  }
}

const { client, isConfigured } = initSupabase()

export const supabase = client
export const isSupabaseConfigured = isConfigured

export async function getAuthToken(): Promise<string | null> {
  if (!supabase) return null
  try {
    const { data } = await supabase.auth.getSession()
    return data.session?.access_token || null
  } catch {
    return null
  }
}
