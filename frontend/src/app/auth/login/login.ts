import { Component, signal } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { HttpErrorResponse } from '@angular/common/http';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.css',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
  ],
})
export class LoginComponent {
  readonly isLoading = signal(false);
  readonly loginError = signal<string | null>(null);
  readonly hidePassword = signal(true);

  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly authService: AuthService
  ) {
    this.form = this.fb.nonNullable.group({
      email: [
        '',
        [
          Validators.required,
          Validators.email,
          Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/),
          Validators.maxLength(256),
        ],
      ],
      password: ['', [Validators.required]],
    });
  }

  get email(): FormControl<string> {
    return this.form.controls.email;
  }

  get password(): FormControl<string> {
    return this.form.controls.password;
  }

  onSubmit(): void {
    this.loginError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const rawValue = this.form.getRawValue();

    this.isLoading.set(true);

    this.authService
      .login({
        email: rawValue.email.trim(),
        password: rawValue.password,
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          console.error(error);
          this.loginError.set(this.getErrorMessage(error));
        },
      });
  }

  togglePasswordVisibility(): void {
    this.hidePassword.update((value) => !value);
  }

  emailErrorMessage(): string | null {
    if (!this.shouldShowError(this.email)) {
      return null;
    }

    if (this.email.hasError('required')) {
      return 'El email es obligatorio.';
    }

    if (this.email.hasError('email') || this.email.hasError('pattern')) {
      return 'Ingresá un email válido.';
    }

    if (this.email.hasError('maxlength')) {
      return 'El email no puede superar los 256 caracteres.';
    }

    return null;
  }

  passwordErrorMessage(): string | null {
    if (!this.shouldShowError(this.password)) {
      return null;
    }

    if (this.password.hasError('required')) {
      return 'La contraseña es obligatoria.';
    }

    return null;
  }

  private shouldShowError(control: FormControl): boolean {
    return control.invalid && (control.touched || control.dirty);
  }

  private getErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'Ocurrió un error inesperado.';
    }

    if (typeof error.error === 'string') {
      return error.error;
    }

    if (error.error?.message) {
      return error.error.message;
    }

    return 'Email o contraseña incorrectos.';
  }
}