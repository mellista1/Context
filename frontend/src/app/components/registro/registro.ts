import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../auth/services/auth.service';
import { OrderService } from '../../services/order.service';
import { OrderResponse, Product } from '../../models/order.models';

@Component({
  selector: 'app-registro',
  imports: [CommonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './registro.html',
  styleUrl: './registro.css',
})
export class RegistroComponent implements OnInit {

  readonly orders = signal<OrderResponse[]>([]);
  readonly isLoadingOrders = signal(false);

  constructor(
    private readonly authService: AuthService,
    private readonly orderService: OrderService,
    readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  getProductNames(products: Product[]): string {
    return products.map(p => p.name).join(', ');
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private loadOrders(): void {
    this.isLoadingOrders.set(true);
    this.orderService
      .getAll()
      .pipe(finalize(() => this.isLoadingOrders.set(false)))
      .subscribe({
        next: orders => this.orders.set(orders),
        error: err => console.error('Error loading orders', err),
      });
  }
}
