import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login'
import { DashboardComponent } from './components/dashboard/dashboard';
import { SignUpComponent } from './auth/sign-up/sign-up'
import { OrdenarComponent } from './components/ordenar/ordenar';
import { RegistroComponent } from './components/registro/registro';
import { AnalisisComponent } from './components/analisis/analisis';

export const routes: Routes = [
    {
        path: '',
        redirectTo: '/dashboard',
        pathMatch: 'full'
    },
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: 'sign-up',
        component: SignUpComponent
    },
    {
        path: 'dashboard',
        component: DashboardComponent
    },
    {
        path: 'ordenar',
        component: OrdenarComponent
    },
    {
        path: 'registro',
        component: RegistroComponent
    },
    {
        path: 'analisis',
        component: AnalisisComponent
    }
];
