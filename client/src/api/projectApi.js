import api from './axiosConfig'

const projectApi = {
  getMyCreatedProjects: () => api.get('/api/project/my-createdprojects'),
  getMyJoinedProjects: () => api.get('/api/project/my-joinedprojects'),
  getProjectById: (id) => api.get(`/api/project/${id}`),
  createProject: (data) => api.post('/api/project/create-project', data),
  updateProject: (data) => api.put('/api/project/update-project', data),
  deleteProject: (id) => api.delete(`/api/project/${id}`),
  getRecommendedProjects: () => api.get('/api/project/recommended-projects'),
  removeContributor: (projectId, studentId) =>
    api.delete(`/api/project/${projectId}/contributors/${studentId}`),
  // Existing search from Projects.jsx uses /api/project/search
  searchProjects: (filter) => api.post('/api/project/search', filter),
}

export default projectApi
