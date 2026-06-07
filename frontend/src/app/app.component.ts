import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from './components/nav/nav.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavComponent],
  template: `
    <app-nav></app-nav>
    <main class="container py-4">
      <router-outlet></router-outlet>
    </main>
  `
})
export class AppComponent {}
