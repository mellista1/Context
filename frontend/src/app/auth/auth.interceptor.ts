import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/services/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const publicEndpoints = [
    '/auth/login',
    '/auth/register',
  ];

  const isPublicEndpoint = publicEndpoints.some((endpoint) =>
    request.url.includes(endpoint)
  );

  if (!token || isPublicEndpoint) {
    return next(request);
  }

  const authenticatedRequest = request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authenticatedRequest);
};