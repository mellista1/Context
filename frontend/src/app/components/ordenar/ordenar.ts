import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../auth/services/auth.service';
import { ProductService } from '../../services/product.service';
import { OrderService } from '../../services/order.service';
import { OrderItem, Product } from '../../models/order.models';

@Component({
  selector: 'app-ordenar',
  imports: [CommonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './ordenar.html',
  styleUrl: './ordenar.css',
})
export class OrdenarComponent implements OnInit {

  readonly products = signal<Product[]>([]);
  readonly quantities = signal<Map<number, number>>(new Map());

  tableNumber: number | null = null;

  readonly isSubmitting = signal(false);
  readonly orderError = signal<string | null>(null);
  readonly orderSuccess = signal(false);
  readonly isLoadingProducts = signal(false);
  readonly isProductListOpen = signal(false);

  readonly orderItems = computed<OrderItem[]>(() =>
    this.products()
      .filter(p => (this.quantities().get(p.id) ?? 0) > 0)
      .map(p => ({ product: p, quantity: this.quantities().get(p.id)! }))
  );

  readonly totalPrice = computed(() =>
    this.orderItems().reduce((sum, item) => sum + item.product.price * item.quantity, 0)
  );

  readonly tables = [1, 2, 3, 4, 5, 6];

  get cannotSubmit(): boolean {
    return (
      this.isSubmitting() ||
      this.orderItems().length === 0 ||
      !this.tableNumber ||
      this.tableNumber < 1
    );
  }

  constructor(
    private readonly authService: AuthService,
    private readonly productService: ProductService,
    private readonly orderService: OrderService,
    readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  getQuantity(productId: number): number {
    return this.quantities().get(productId) ?? 0;
  }

  addProduct(product: Product): void {
    const current = this.quantities();
    const qty = current.get(product.id) ?? 0;
    this.quantities.set(new Map(current).set(product.id, qty + 1));
  }

  removeProduct(productId: number): void {
    const current = this.quantities();
    const qty = current.get(productId) ?? 0;
    const next = new Map(current);
    if (qty <= 1) next.delete(productId);
    else next.set(productId, qty - 1);
    this.quantities.set(next);
  }

  selectTable(n: number): void {
    this.tableNumber = n;
  }

  openProductList(): void {
    this.isProductListOpen.set(true);
  }

  closeProductList(): void {
    this.isProductListOpen.set(false);
  }

  createOrder(): void {
    if (!this.tableNumber || this.tableNumber < 1 || this.orderItems().length === 0) return;

    this.orderError.set(null);
    this.orderSuccess.set(false);
    this.isSubmitting.set(true);

    const productIds = this.orderItems().flatMap(item =>
      Array(item.quantity).fill(item.product.id)
    );

    this.orderService
      .create({ tableNumber: this.tableNumber, productIds })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.quantities.set(new Map());
          this.tableNumber = null;
          this.orderSuccess.set(true);
          setTimeout(() => this.orderSuccess.set(false), 3000);
        },
        error: err => {
          console.error(err);
          this.orderError.set('No se pudo crear la orden. Intentá de nuevo.');
        },
      });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private loadProducts(): void {
    this.isLoadingProducts.set(true);
    this.productService
      .getAll()
      .pipe(finalize(() => this.isLoadingProducts.set(false)))
      .subscribe({
        next: products => this.products.set(products),
        error: err => console.error('Error loading products', err),
      });
  }
}
