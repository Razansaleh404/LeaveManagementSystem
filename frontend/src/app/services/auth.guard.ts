import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Role } from '../models/models';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = (route.data?.['roles'] ?? []) as Role[];

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (roles.length && !auth.hasRole(roles)) {
    return router.createUrlTree(['/history']);
  }

  return true;
};
