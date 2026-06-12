import { Component, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, switchMap } from 'rxjs';

import { HttpErrorResponse } from '@angular/common/http';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../services/auth.service';
import { BusinessService } from '../services/business.service';
import { RegisterRequest } from '../../models/auth.models';
import { CreateBusinessRequest } from '../../models/business.models';

@Component({
  selector: 'app-sign-up',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
  ],
  templateUrl: './sign-up.html',
  styleUrl: './sign-up.css',
})
export class SignUpComponent {
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly hidePassword = signal(true);
  readonly hideConfirmPassword = signal(true);

  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly businessService: BusinessService,
    private readonly router: Router
  ) {
    this.form = this.fb.nonNullable.group(
      {
        fullName: ['', [Validators.required, Validators.maxLength(150)]],
        email: [
          '',
          [
            Validators.required,
            Validators.email,
            Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/),
            Validators.maxLength(256),
          ],
        ],
        password: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', [Validators.required]],

        businessName: ['', [Validators.required, Validators.maxLength(150)]],
        businessDescription: ['', [Validators.maxLength(1000)]],
        businessAddress: ['', [Validators.required, Validators.maxLength(250)]],
      },
      {
        validators: [this.passwordsMatchValidator()],
      }
    );
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.isSubmitting.set(true);

    const rawValue = this.form.getRawValue();

    const registerRequest: RegisterRequest = {
      fullName: rawValue.fullName.trim(),
      email: rawValue.email.trim(),
      password: rawValue.password,
    };

    const createBusinessRequest: CreateBusinessRequest = {
      name: rawValue.businessName.trim(),
      description: rawValue.businessDescription.trim() || undefined,
      address: rawValue.businessAddress.trim(),
    };

    this.authService
      .register(registerRequest)
      .pipe(
        switchMap(() =>
          this.businessService.createBusiness(createBusinessRequest)
        ),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          console.error(error);
          this.errorMessage.set(this.getErrorMessage(error));
        },
      });
  }

  shouldShowError(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.dirty || control.touched);
  }

  togglePasswordVisibility(): void {
    this.hidePassword.update((value) => !value);
  }

  toggleConfirmPasswordVisibility(): void {
    this.hideConfirmPassword.update((value) => !value);
  }

  private passwordsMatchValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const passwordControl = control.get('password');
      const confirmPasswordControl = control.get('confirmPassword');

      if (!passwordControl || !confirmPasswordControl) {
        return null;
      }

      const password = passwordControl.value;
      const confirmPassword = confirmPasswordControl.value;

      if (!password || !confirmPassword) {
        this.removeConfirmPasswordMismatchError(confirmPasswordControl);
        return null;
      }

      if (password !== confirmPassword) {
        confirmPasswordControl.setErrors({
          ...confirmPasswordControl.errors,
          passwordsMismatch: true,
        });

        return { passwordsMismatch: true };
      }

      this.removeConfirmPasswordMismatchError(confirmPasswordControl);
      return null;
    };
  }

  private removeConfirmPasswordMismatchError( control: AbstractControl): void {
    if (!control.errors?.['passwordsMismatch']) { return; }
    const errors = { ...control.errors };
    delete errors['passwordsMismatch'];
    control.setErrors(Object.keys(errors).length ? errors : null);
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

    if (Array.isArray(error.error?.errors)) {
      return error.error.errors
        .map((e: any) => e.description ?? e.message ?? e.code)
        .join('\n');
    }

    if (error.error?.errors) {
      return Object.values(error.error.errors).flat().join('\n');
    }

    return 'No se pudo completar el registro. Revisá los datos e intentá nuevamente.';
  }
}