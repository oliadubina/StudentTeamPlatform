import { Edit2, Plus, Trash2 } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { toast } from 'react-toastify'
import projectApi from '../api/projectApi'
import ProjectCard from '../components/ProjectCard'
import ProjectForm from '../components/ProjectForm'

const TABS = {
  created: 'created',
  joined: 'joined',
}

// Mocked technologies for mapping
const AVAILABLE_TECHNOLOGIES = [
  { id: 1, name: 'React' },
  { id: 2, name: 'Node.js' },
  { id: 3, name: '.NET' },
  { id: 4, name: 'TypeScript' },
  { id: 5, name: 'Python' },
  { id: 6, name: 'PostgreSQL' },
  { id: 7, name: 'Tailwind CSS' },
]

function MyProjects() {
  const [activeTab, setActiveTab] = useState(TABS.created)
  const [projects, setProjects] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingProject, setEditingProject] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const fetchProjects = useCallback(async () => {
    setIsLoading(true)
    try {
      const response =
        activeTab === TABS.created
          ? await projectApi.getMyCreatedProjects()
          : await projectApi.getMyJoinedProjects()
      setProjects(response.data || [])
    } catch {
      toast.error('Не вдалося завантажити проєкти')
    } finally {
      setIsLoading(false)
    }
  }, [activeTab])

  useEffect(() => {
    const timeoutId = setTimeout(fetchProjects, 0)

    return () => clearTimeout(timeoutId)
  }, [fetchProjects])

  const handleCreate = () => {
    setEditingProject(null)
    setIsModalOpen(true)
  }

  const handleEdit = async (projectId) => {
    try {
      const response = await projectApi.getProjectById(projectId)
      const projectData = response.data

      // Map backend data to form structure
      const formattedData = {
        title: projectData.title,
        description: projectData.description,
        maxContributors: projectData.maxContributors,
        projectType: projectData.projectType,
        workFormat: projectData.workFormat,
        language: projectData.language,
        // Map technology names to IDs
        technologyIds: projectData.technology
          ? projectData.technology
              .map((t) => AVAILABLE_TECHNOLOGIES.find((at) => at.name === t.name)?.id)
              .filter(Boolean)
          : [],
        roles: projectData.projectRoles || [{ name: '', slotsCount: 1 }],
      }

      setEditingProject({ id: projectId, ...formattedData })
      setIsModalOpen(true)
    } catch {
      toast.error('Не вдалося завантажити дані проєкту для редагування')
    }
  }

  const handleDelete = async (projectId) => {
    if (!window.confirm('Ви впевнені, що хочете видалити цей проєкт?')) return

    try {
      await projectApi.deleteProject(projectId)
      toast.success('Проєкт успішно видалено')
      fetchProjects()
    } catch (error) {
      toast.error(error.response?.data || 'Не вдалося видалити проєкт')
    }
  }

  const handleSubmit = async (data) => {
    setIsSubmitting(true)
    try {
      if (editingProject) {
        // Update
        const payload = {
          ...data,
          id: editingProject.id,
          // UpdateProjectDTO expects Technologies as List<TechnologyDTO> { Name }
          technologies: data.technologyIds.map((id) => ({
            name: AVAILABLE_TECHNOLOGIES.find((t) => t.id === id)?.name || '',
          })),
          projectState: editingProject.projectState || 0, // Keep state or default to 0
        }
        await projectApi.updateProject(payload)
        toast.success('Проєкт успішно оновлено')
      } else {
        // Create
        await projectApi.createProject(data)
        toast.success('Проєкт успішно створено')
      }
      setIsModalOpen(false)
      fetchProjects()
    } catch (error) {
      toast.error(error.response?.data || 'Помилка при збереженні проєкту')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-950">Мої проєкти</h1>
          <p className="mt-2 text-slate-600">
            Керуйте власними проєктами або переглядайте ті, до яких ви долучилися.
          </p>
        </div>
        {activeTab === TABS.created && (
          <button
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 shadow-md transition-all active:scale-95"
            onClick={handleCreate}
          >
            <Plus size={20} /> Створити проєкт
          </button>
        )}
      </div>

      <div className="inline-flex rounded-xl bg-slate-100 p-1 shadow-inner">
        <button
          className={`rounded-lg px-6 py-2.5 text-sm font-semibold transition-all ${
            activeTab === TABS.created
              ? 'bg-white text-slate-950 shadow-sm'
              : 'text-slate-500 hover:text-slate-950'
          }`}
          onClick={() => setActiveTab(TABS.created)}
        >
          Створені мною
        </button>
        <button
          className={`rounded-lg px-6 py-2.5 text-sm font-semibold transition-all ${
            activeTab === TABS.joined
              ? 'bg-white text-slate-950 shadow-sm'
              : 'text-slate-500 hover:text-slate-950'
          }`}
          onClick={() => setActiveTab(TABS.joined)}
        >
          Моя участь
        </button>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-64 animate-pulse rounded-xl bg-slate-100" />
          ))}
        </div>
      ) : projects.length > 0 ? (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {projects.map((project, index) => {
            const projectId = project.id ?? project.Id
            return (
              <ProjectCard
                key={projectId ?? `project-${index}`}
                actions={
                  activeTab === TABS.created && (
                    <div className="flex gap-2">
                      <button
                        className="rounded-lg bg-slate-100 p-2 text-slate-600 hover:bg-blue-50 hover:text-blue-600 transition-colors"
                        onClick={() => handleEdit(projectId)}
                        title="Редагувати"
                      >
                        <Edit2 size={18} />
                      </button>
                      <button
                        className="rounded-lg bg-slate-100 p-2 text-slate-600 hover:bg-red-50 hover:text-red-600 transition-colors"
                        onClick={() => handleDelete(projectId)}
                        title="Видалити"
                      >
                        <Trash2 size={18} />
                      </button>
                    </div>
                  )
                }
                project={project}
              />
            )
          })}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-slate-200 bg-slate-50/50 py-16 text-center">
          <p className="text-lg font-medium text-slate-600">
            {activeTab === TABS.created
              ? 'Ви ще не створили жодного проєкту'
              : 'Ви ще не долучилися до жодного проєкту'}
          </p>
          {activeTab === TABS.created && (
            <button
              className="mt-4 text-sm font-semibold text-blue-600 hover:text-blue-700"
              onClick={handleCreate}
            >
              Створіть свій перший проєкт прямо зараз
            </button>
          )}
        </div>
      )}

      {/* Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
          <div className="w-full max-w-2xl max-h-[90vh] overflow-y-auto rounded-2xl bg-white shadow-2xl">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-100 bg-white px-6 py-4">
              <h2 className="text-xl font-bold text-slate-950">
                {editingProject ? 'Редагувати проєкт' : 'Створити новий проєкт'}
              </h2>
              <button
                className="rounded-full p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
                onClick={() => setIsModalOpen(false)}
              >
                <Plus className="rotate-45" size={24} />
              </button>
            </div>
            <div className="p-6">
              <ProjectForm
                initialData={editingProject}
                isSubmitting={isSubmitting}
                onCancel={() => setIsModalOpen(false)}
                onSubmit={handleSubmit}
              />
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default MyProjects
