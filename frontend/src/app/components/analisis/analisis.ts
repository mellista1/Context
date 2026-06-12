import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../../auth/services/auth.service';

interface MonthlySale {
  month: string;
  amount: number;
}

interface TopProduct {
  name: string;
  units: number;
}

interface AiSuggestion {
  category: 'new_product' | 'best_seller' | 'customer_review';
  title: string;
  body: string;
}

const MOCK_MONTHLY_SALES: MonthlySale[] = [
  { month: 'Ene', amount: 185000 },
  { month: 'Feb', amount: 210000 },
  { month: 'Mar', amount: 195000 },
  { month: 'Abr', amount: 230000 },
  { month: 'May', amount: 275000 },
  { month: 'Jun', amount: 258000 },
];

const MOCK_TOP_PRODUCTS: TopProduct[] = [
  { name: 'Empanadas de carne', units: 340 },
  { name: 'Milanesa napolitana', units: 290 },
  { name: 'Pizza muzarella', units: 265 },
  { name: 'Medialunas', units: 220 },
  { name: 'Café con leche', units: 195 },
];

const MOCK_SUGGESTIONS: AiSuggestion[] = [
  {
    category: 'new_product',
    title: 'Incorporá una opción vegana',
    body: 'El 34% de tus clientes tienen entre 20 y 35 años y la demanda de opciones plant-based creció un 40% en el último trimestre. Agregar una empanada de verduras premium podría capturar este segmento.',
  },
  {
    category: 'best_seller',
    title: 'Impulsá tus empanadas en el mediodía',
    body: 'Las empanadas de carne son tu producto estrella y el 68% de sus ventas ocurre entre las 12 y las 15 hs. Un combo almuerzo podría aumentar el ticket promedio.',
  },
  {
    category: 'customer_review',
    title: 'Respondé las reseñas de Google Maps',
    body: 'Tenés 8 reseñas sin responder de los últimos 30 días. Los negocios que responden sistemáticamente tienen en promedio 0.4 puntos más de calificación.',
  },
  {
    category: 'new_product',
    title: 'Sumá bebidas artesanales',
    body: 'Las cervezas artesanales y limonadas naturales están en tendencia en tu zona. Incorporarlas al menú podría aumentar el ticket promedio entre un 15 y un 20%.',
  },
  {
    category: 'best_seller',
    title: 'Destacá las medialunas en el menú digital',
    body: 'Las medialunas tienen alto margen y buena rotación en desayunos. Ponerlas como destacado puede incrementar sus ventas hasta un 25%.',
  },
  {
    category: 'customer_review',
    title: 'Mejorá los tiempos de espera en horas pico',
    body: 'El 23% de las reseñas recientes mencionan demoras entre las 12 y las 14 hs. Reforzar el personal en ese horario puede mejorar la calificación general del local.',
  },
];

@Component({
  selector: 'app-analisis',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './analisis.html',
  styleUrl: './analisis.css',
})
export class AnalisisComponent {
  readonly monthlySales = MOCK_MONTHLY_SALES;
  readonly topProducts = MOCK_TOP_PRODUCTS;
  readonly suggestions = MOCK_SUGGESTIONS;

  private get maxSales(): number {
    return Math.max(...this.monthlySales.map((s) => s.amount));
  }

  private get maxUnits(): number {
    return Math.max(...this.topProducts.map((p) => p.units));
  }

  get totalThisMonth(): string {
    const last = this.monthlySales[this.monthlySales.length - 1];
    return `$${last.amount.toLocaleString('es-AR')}`;
  }

  get topProductName(): string {
    return this.topProducts[0].name;
  }

  get avgTicket(): string {
    const total = this.monthlySales.reduce((acc, s) => acc + s.amount, 0);
    const avg = Math.round(total / this.monthlySales.length);
    return `$${avg.toLocaleString('es-AR')}`;
  }

  salesBarHeight(amount: number): number {
    return Math.round((amount / this.maxSales) * 85);
  }

  productBarWidth(units: number): number {
    return Math.round((units / this.maxUnits) * 100);
  }

  categoryLabel(cat: AiSuggestion['category']): string {
    const labels: Record<AiSuggestion['category'], string> = {
      new_product: 'Nuevo producto',
      best_seller: 'Más vendido',
      customer_review: 'Reseñas',
    };
    return labels[cat];
  }

  categoryIcon(cat: AiSuggestion['category']): string {
    const icons: Record<AiSuggestion['category'], string> = {
      new_product: 'add_circle',
      best_seller: 'emoji_events',
      customer_review: 'star',
    };
    return icons[cat];
  }

  constructor(
    readonly router: Router,
    private readonly authService: AuthService,
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
