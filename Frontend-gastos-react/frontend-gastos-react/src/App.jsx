import { useEffect, useMemo, useState, useCallback } from 'react'
import './styles.css'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

// ── Helpers ────────────────────────────────────────────────────────────────
function formatDateInput(date) {
  if (!date) return ''
  const d = new Date(date)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function formatDateDisplay(date) {
  if (!date) return '—'
  return new Date(date).toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' })
}

function formatCurrency(amount) {
  return new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(amount)
}

// ── API helper ─────────────────────────────────────────────────────────────
async function apiFetch(path, options = {}) {
  const res = await fetch(`${API_URL}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', ...(options.headers || {}) },
    ...options,
  })
  const isJson = res.headers.get('content-type')?.includes('application/json')
  const body = isJson ? await res.json().catch(() => ({})) : null
  if (!res.ok) throw new Error(body?.message || `Error ${res.status}`)
  return body
}

// ── Componentes pequeños ───────────────────────────────────────────────────
function Spinner() {
  return (
    <div className="center-screen">
      <div className="loader-wrap">
        <div className="spinner" />
        <span>Cargando…</span>
      </div>
    </div>
  )
}

function Alert({ type = 'error', message, onClose }) {
  if (!message) return null
  const icon = type === 'error' ? '⚠️' : '✅'
  return (
    <div className={`alert alert-${type}`}>
      <span>{icon}</span>
      <span style={{ flex: 1 }}>{message}</span>
      {onClose && <button onClick={onClose} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: 16, color: 'inherit', padding: '0 4px' }}>✕</button>}
    </div>
  )
}

function RoleBadge({ role }) {
  return <span className={`role-badge ${role?.toLowerCase()}`}>{role}</span>
}

function SectionHeader({ icon, title, colorClass }) {
  return (
    <div className="section-header">
      <div className={`section-icon ${colorClass}`}>{icon}</div>
      <h2>{title}</h2>
    </div>
  )
}

// ── Login ──────────────────────────────────────────────────────────────────
function LoginPage({ onLogin }) {
  const [form, setForm] = useState({ email: 'admin@gastos.com', password: 'Admin123*' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await apiFetch('/auth/login', { method: 'POST', body: JSON.stringify(form) })
      const me = await apiFetch('/auth/me')
      onLogin(me)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="center-screen">
      <div className="login-card">
        <div className="login-logo">
          <div className="logo-icon">💰</div>
          <h1>Gestión de Gastos</h1>
          <p>Inicia sesión para continuar</p>
        </div>

        <Alert type="error" message={error} onClose={() => setError('')} />

        <form className="form-grid" onSubmit={handleSubmit}>
          <div className="field">
            <label>Correo electrónico</label>
            <input
              type="email"
              placeholder="correo@ejemplo.com"
              value={form.email}
              onChange={e => setForm({ ...form, email: e.target.value })}
              required
            />
          </div>
          <div className="field">
            <label>Contraseña</label>
            <input
              type="password"
              placeholder="••••••••"
              value={form.password}
              onChange={e => setForm({ ...form, password: e.target.value })}
              required
            />
          </div>
          <button type="submit" className="btn btn-primary btn-full" disabled={loading} style={{ marginTop: 4 }}>
            {loading ? 'Iniciando sesión…' : 'Iniciar sesión'}
          </button>
        </form>

        <p style={{ textAlign: 'center', fontSize: 12, color: 'var(--gray-400)', marginTop: 20 }}>
          Sistema de Examen · Backend C# + EF Core
        </p>
      </div>
    </div>
  )
}

// ── App principal ──────────────────────────────────────────────────────────
export default function App() {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  const [categories, setCategories] = useState([])
  const [expenses, setExpenses]     = useState([])

  const [categoryFilter, setCategoryFilter] = useState('')
  const [feedback, setFeedback] = useState({ type: '', msg: '' })

  // Formulario gasto
  const emptyExpense = { concept: '', amount: '', expenseDate: '', categoryId: '' }
  const [expenseForm, setExpenseForm]         = useState(emptyExpense)
  const [editingExpenseId, setEditingExpenseId] = useState(null)
  const [savingExpense, setSavingExpense]       = useState(false)

  // Formulario categoría
  const [categoryForm, setCategoryForm]           = useState({ name: '' })
  const [editingCategoryId, setEditingCategoryId]  = useState(null)
  const [savingCategory, setSavingCategory]         = useState(false)

  // Stats calculadas
  const total = useMemo(() => expenses.reduce((s, e) => s + Number(e.amount || 0), 0), [expenses])
  const uniqueUsers = useMemo(() => new Set(expenses.map(e => e.userId)).size, [expenses])

  // ── Feedback helpers ──
  function showOk(msg)  { setFeedback({ type: 'success', msg }); setTimeout(() => setFeedback({ type: '', msg: '' }), 3000) }
  function showErr(msg) { setFeedback({ type: 'error', msg }) }
  function clearFb()    { setFeedback({ type: '', msg: '' }) }

  // ── Carga inicial ──
  async function loadSession() {
    setLoading(true)
    try {
      const me = await apiFetch('/auth/me')
      setUser(me)
      await Promise.all([loadCategories(), loadExpenses('')])
    } catch {
      setUser(null)
    } finally {
      setLoading(false)
    }
  }

  const loadCategories = useCallback(async () => {
    try {
      const data = await apiFetch('/categories')
      setCategories(Array.isArray(data) ? data : [])
    } catch (err) { showErr(err.message) }
  }, [])

  const loadExpenses = useCallback(async (catId = categoryFilter) => {
    try {
      const q = catId ? `?categoryId=${catId}` : ''
      const data = await apiFetch(`/expenses${q}`)
      setExpenses(Array.isArray(data) ? data : [])
    } catch (err) { showErr(err.message) }
  }, [categoryFilter])

  useEffect(() => { loadSession() }, [])
  useEffect(() => { if (user) loadExpenses(categoryFilter) }, [categoryFilter])

  // ── Auth ──
  async function handleLogout() {
    try { await apiFetch('/auth/logout', { method: 'POST' }) } catch {}
    setUser(null)
    setCategories([])
    setExpenses([])
    setEditingExpenseId(null)
    setEditingCategoryId(null)
    setCategoryFilter('')
    setExpenseForm(emptyExpense)
    setCategoryForm({ name: '' })
  }

  // ── CRUD Gastos ──
  function startEditExpense(item) {
    setEditingExpenseId(item.id)
    setExpenseForm({
      concept: item.concept,
      amount: item.amount,
      expenseDate: formatDateInput(item.expenseDate),
      categoryId: `${item.categoryId}`
    })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function cancelEditExpense() {
    setEditingExpenseId(null)
    setExpenseForm(emptyExpense)
  }

  async function handleSubmitExpense(e) {
    e.preventDefault()
    clearFb()
    setSavingExpense(true)
    try {
      const payload = {
        concept: expenseForm.concept,
        amount: Number(expenseForm.amount),
        expenseDate: new Date(expenseForm.expenseDate).toISOString(),
        categoryId: Number(expenseForm.categoryId)
      }
      if (editingExpenseId) {
        await apiFetch(`/expenses/${editingExpenseId}`, { method: 'PUT', body: JSON.stringify(payload) })
        showOk('Gasto actualizado correctamente.')
      } else {
        await apiFetch('/expenses', { method: 'POST', body: JSON.stringify(payload) })
        showOk('Gasto registrado correctamente.')
      }
      cancelEditExpense()
      await loadExpenses(categoryFilter)
    } catch (err) {
      showErr(err.message)
    } finally {
      setSavingExpense(false)
    }
  }

  async function handleDeleteExpense(id) {
    if (!window.confirm('¿Seguro que deseas eliminar este gasto? Esta acción no se puede deshacer.')) return
    clearFb()
    try {
      await apiFetch(`/expenses/${id}`, { method: 'DELETE' })
      showOk('Gasto eliminado.')
      await loadExpenses(categoryFilter)
    } catch (err) { showErr(err.message) }
  }

  // ── CRUD Categorías ──
  function startEditCategory(item) {
    setEditingCategoryId(item.id)
    setCategoryForm({ name: item.name })
  }

  function cancelEditCategory() {
    setEditingCategoryId(null)
    setCategoryForm({ name: '' })
  }

  async function handleSubmitCategory(e) {
    e.preventDefault()
    clearFb()
    setSavingCategory(true)
    try {
      if (editingCategoryId) {
        await apiFetch(`/categories/${editingCategoryId}`, { method: 'PUT', body: JSON.stringify(categoryForm) })
        showOk('Categoría actualizada.')
      } else {
        await apiFetch('/categories', { method: 'POST', body: JSON.stringify(categoryForm) })
        showOk('Categoría creada.')
      }
      cancelEditCategory()
      await loadCategories()
    } catch (err) {
      showErr(err.message)
    } finally {
      setSavingCategory(false)
    }
  }

  async function handleDeleteCategory(id) {
    if (!window.confirm('¿Seguro que deseas eliminar esta categoría?')) return
    clearFb()
    try {
      await apiFetch(`/categories/${id}`, { method: 'DELETE' })
      showOk('Categoría eliminada.')
      if (`${id}` === categoryFilter) setCategoryFilter('')
      await Promise.all([loadCategories(), loadExpenses('')])
    } catch (err) { showErr(err.message) }
  }

  // ── Render guards ──
  if (loading) return <Spinner />
  if (!user) return <LoginPage onLogin={u => { setUser(u); loadCategories(); loadExpenses('') }} />

  const isAdmin = user.role === 'Admin'

  // ── Dashboard ──
  return (
    <div className="page">
      {/* Topbar */}
      <header className="topbar">
        <div className="topbar-left">
          <h1>💰 Gestión de Gastos</h1>
          <div className="user-info">
            <span>{user.fullName}</span>
            <span>·</span>
            <span>{user.email}</span>
            <span>·</span>
            <RoleBadge role={user.role} />
          </div>
        </div>
        <button className="btn btn-secondary" onClick={handleLogout}>
          🚪 Cerrar sesión
        </button>
      </header>

      {/* Feedback global */}
      <Alert type={feedback.type} message={feedback.msg} onClose={clearFb} />

      {/* Stats */}
      <div className="grid stats-row" style={{ marginBottom: 20 }}>
        <div className="card stat-card">
          <div className="stat-value">{expenses.length}</div>
          <div className="stat-label">{isAdmin ? 'Gastos en total' : 'Mis gastos'}</div>
        </div>
        <div className="card stat-card">
          <div className="stat-value" style={{ fontSize: 22 }}>{formatCurrency(total)}</div>
          <div className="stat-label">Monto acumulado</div>
        </div>
        <div className="card stat-card">
          {isAdmin
            ? <><div className="stat-value">{uniqueUsers}</div><div className="stat-label">Usuarios con gastos</div></>
            : <><div className="stat-value">{categories.length}</div><div className="stat-label">Categorías disponibles</div></>
          }
        </div>
      </div>

      {/* Panel principal */}
      <div className={`grid ${isAdmin ? 'three-cols' : 'two-cols'}`}>

        {/* Formulario de gasto */}
        <div className="card">
          <SectionHeader
            icon={editingExpenseId ? '✏️' : '➕'}
            title={editingExpenseId ? 'Editar gasto' : 'Registrar nuevo gasto'}
            colorClass="icon-blue"
          />
          <form className="form-grid" onSubmit={handleSubmitExpense}>
            <div className="field">
              <label>Concepto</label>
              <input
                type="text"
                placeholder="Ej: Almuerzo, transporte…"
                value={expenseForm.concept}
                onChange={e => setExpenseForm({ ...expenseForm, concept: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label>Valor (COP)</label>
              <input
                type="number"
                step="0.01"
                min="0"
                placeholder="0.00"
                value={expenseForm.amount}
                onChange={e => setExpenseForm({ ...expenseForm, amount: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label>Fecha del gasto</label>
              <input
                type="date"
                value={expenseForm.expenseDate}
                onChange={e => setExpenseForm({ ...expenseForm, expenseDate: e.target.value })}
                required
              />
            </div>
            <div className="field">
              <label>Categoría</label>
              <select
                value={expenseForm.categoryId}
                onChange={e => setExpenseForm({ ...expenseForm, categoryId: e.target.value })}
                required
              >
                <option value="">Selecciona una categoría…</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
            <div className="btn-row">
              <button type="submit" className="btn btn-primary" disabled={savingExpense} style={{ flex: 1 }}>
                {savingExpense ? 'Guardando…' : editingExpenseId ? '💾 Actualizar' : '✅ Registrar'}
              </button>
              {editingExpenseId && (
                <button type="button" className="btn btn-secondary" onClick={cancelEditExpense}>
                  Cancelar
                </button>
              )}
            </div>
          </form>
        </div>

        {/* Resumen y filtro */}
        <div className="card">
          <SectionHeader icon="📊" title="Resumen y Filtro" colorClass="icon-amber" />

          <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            <div style={{ padding: '14px 16px', background: 'var(--blue-50)', borderRadius: 10, border: '1px solid var(--blue-100)' }}>
              <div style={{ fontSize: 12, color: 'var(--gray-500)', marginBottom: 2 }}>{isAdmin ? 'Total de gastos (todos los usuarios)' : 'Total de mis gastos'}</div>
              <div style={{ fontSize: 22, fontWeight: 800, color: 'var(--blue-600)' }}>{formatCurrency(total)}</div>
            </div>
            <div style={{ padding: '12px 16px', background: 'var(--gray-50)', borderRadius: 10, border: '1px solid var(--gray-200)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: 13, color: 'var(--gray-500)' }}>Registros visibles</span>
              <span style={{ fontSize: 18, fontWeight: 700 }}>{expenses.length}</span>
            </div>
          </div>

          <div className="filter-row">
            <label>🔍 Filtrar:</label>
            <select value={categoryFilter} onChange={e => setCategoryFilter(e.target.value)}>
              <option value="">Todas las categorías</option>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            {categoryFilter && (
              <button className="btn btn-secondary btn-sm" onClick={() => setCategoryFilter('')}>✕</button>
            )}
          </div>
        </div>

        {/* Panel de categorías (solo Admin) */}
        {isAdmin && (
          <div className="card">
            <SectionHeader
              icon={editingCategoryId ? '✏️' : '🏷️'}
              title={editingCategoryId ? 'Editar categoría' : 'Categorías'}
              colorClass="icon-purple"
            />
            <form className="form-grid" onSubmit={handleSubmitCategory}>
              <div className="field">
                <label>{editingCategoryId ? 'Nuevo nombre' : 'Nombre de la categoría'}</label>
                <input
                  type="text"
                  placeholder="Ej: Entretenimiento"
                  value={categoryForm.name}
                  onChange={e => setCategoryForm({ name: e.target.value })}
                  required
                />
              </div>
              <div className="btn-row">
                <button type="submit" className="btn btn-primary" disabled={savingCategory} style={{ flex: 1 }}>
                  {savingCategory ? 'Guardando…' : editingCategoryId ? '💾 Actualizar' : '➕ Crear'}
                </button>
                {editingCategoryId && (
                  <button type="button" className="btn btn-secondary" onClick={cancelEditCategory}>Cancelar</button>
                )}
              </div>
            </form>

            <div className="divider" />
            <div className="mini-list">
              {categories.length === 0 && (
                <p style={{ textAlign: 'center', color: 'var(--gray-400)', fontSize: 13, padding: 12 }}>No hay categorías aún.</p>
              )}
              {categories.map(c => (
                <div key={c.id} className="mini-item">
                  <span className="mini-item-name">🏷️ {c.name}</span>
                  <div className="mini-item-actions">
                    <button className="btn btn-secondary btn-sm" onClick={() => startEditCategory(c)}>✏️</button>
                    <button className="btn btn-danger btn-sm" onClick={() => handleDeleteCategory(c.id)}>🗑️</button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Tabla de gastos */}
      <div className="card table-section">
        <SectionHeader icon="📋" title="Listado de gastos" colorClass="icon-green" />

        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>#</th>
                <th>Concepto</th>
                <th>Categoría</th>
                <th>Valor</th>
                <th>Fecha</th>
                {isAdmin && <th>Usuario</th>}
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {expenses.length === 0 ? (
                <tr className="empty-row">
                  <td colSpan={isAdmin ? 7 : 6}>
                    <div className="empty-icon">💸</div>
                    <div className="empty-text">No hay gastos registrados</div>
                    <div className="empty-sub">
                      {categoryFilter ? 'Prueba quitando el filtro de categoría.' : 'Usa el formulario de la izquierda para registrar el primer gasto.'}
                    </div>
                  </td>
                </tr>
              ) : (
                expenses.map(item => (
                  <tr key={item.id}>
                    <td style={{ color: 'var(--gray-400)', fontSize: 13 }}>{item.id}</td>
                    <td className="concept-cell">{item.concept}</td>
                    <td><span className="cat-chip">{item.categoryName || `Cat. ${item.categoryId}`}</span></td>
                    <td className="amount-cell">{formatCurrency(item.amount)}</td>
                    <td>{formatDateDisplay(item.expenseDate)}</td>
                    {isAdmin && <td><span className="user-chip">👤 {item.userName || `Usuario ${item.userId}`}</span></td>}
                    <td>
                      <div className="table-actions">
                        <button className="btn btn-secondary btn-sm" onClick={() => startEditExpense(item)}>✏️ Editar</button>
                        <button className="btn btn-danger btn-sm" onClick={() => handleDeleteExpense(item.id)}>🗑️</button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
