import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserRole } from '../models/models';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const allowedRoles = route.data?.['roles'] as UserRole[] | undefined;

  if (auth.canAccess(allowedRoles)) return true;

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url === '/' ? undefined : state.url }
  });
};
