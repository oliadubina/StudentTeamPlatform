import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { ToastContainer } from 'react-toastify'
import 'react-toastify/dist/ReactToastify.css'
import Navbar from './components/Navbar'
import Chats from './pages/Chats'
import Dashboard from './pages/Dashboard'
import ExternalProfile from './pages/ExternalProfile'
import Login from './pages/Login'
import MyProjects from './pages/MyProjects'
import MyRequests from './pages/MyRequests'
import Profile from './pages/Profile'
import ProjectDetails from './pages/ProjectDetails'
import Projects from './pages/Projects'
import Register from './pages/Register'

function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <main className="mx-auto w-full max-w-6xl px-4 py-8">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/chats" element={<Chats />} />
          <Route path="/login" element={<Login />} />
          <Route path="/my-projects" element={<MyProjects />} />
          <Route path="/my-requests" element={<MyRequests />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/projects" element={<Projects />} />
          <Route path="/projects/:projectId" element={<ProjectDetails />} />
          <Route path="/register" element={<Register />} />
          <Route path="/users/:userId" element={<ExternalProfile />} />
        </Routes>
      </main>
      <ToastContainer position="top-right" autoClose={3000} />
    </BrowserRouter>
  )
}

export default App
