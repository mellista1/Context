import { Component } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { finalize, Observable } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../../auth/services/auth.service';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { DeleteAccountDialogComponent } from '../delete-account-dialog/delete-account-dialog';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    AsyncPipe,
    RouterLink,
    RouterLinkActive,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class NavbarComponent {
  readonly loggedIn$: Observable<boolean>;
  readonly fullName$: Observable<string | null>;
  isDeletingAccount = false;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly dialog: MatDialog
  ) {
    this.loggedIn$ = this.authService.loggedIn$;
    this.fullName$ = this.authService.fullName$;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  openDeleteAccountDialog(): void {
    const dialogRef = this.dialog.open(DeleteAccountDialogComponent, {
      width: '420px',
      maxWidth: 'calc(100vw - 32px)',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) {
        return;
      }

      this.deleteAccount();
    });
  }

  private deleteAccount(): void {
    this.isDeletingAccount = true;

    this.authService
      .deleteMyAccount()
      .pipe(finalize(() => (this.isDeletingAccount = false)))
      .subscribe({
        next: () => {
          this.authService.logout();
          this.router.navigate(['/login']);
        },
        error: (error) => {
          console.error('Error deleting account', error);
        },
      });
  }
}