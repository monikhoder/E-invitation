import { createRouter, createWebHistory } from 'vue-router'
import InvitationView from '../views/InvitationView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'invitation',
      component: InvitationView,
    },
    {
      path: '/invite/:token',
      name: 'guest-invitation',
      component: InvitationView,
    },
    {
      path: '/admin/login',
      name: 'admin-login',
      component: () => import('../views/AdminLoginView.vue'),
    },
    {
      path: '/admin/dashboard',
      name: 'admin-dashboard',
      component: () => import('../views/AdminDashboardView.vue'),
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

export default router
