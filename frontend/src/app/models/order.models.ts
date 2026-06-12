export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
}

export interface OrderItem {
  product: Product;
  quantity: number;
}

export interface CreateOrderRequest {
  tableNumber: number;
  productIds: number[];
}

export interface OrderResponse {
  id: number;
  tableNumber: number;
  createdAt: string;
  products: Product[];
  totalPrice: number;
}
