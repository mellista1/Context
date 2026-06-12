import { Component, signal } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';

import { NavbarComponent } from './components/navbar/navbar';
import { AssistantBubbleComponent } from './components/assistant-bubble/assistant-bubble';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, AssistantBubbleComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  readonly isDashboard = signal(false);

  private isAppShellRoute(url: string): boolean {
    return url.startsWith('/dashboard') || url.startsWith('/ordenar') || url.startsWith('/registro') || url.startsWith('/analisis');
  }

  constructor(private readonly router: Router) {
    this.isDashboard.set(this.isAppShellRoute(window.location.pathname));

    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe((e: any) => {
        this.isDashboard.set(this.isAppShellRoute((e as NavigationEnd).urlAfterRedirects));
      });
  }
}
