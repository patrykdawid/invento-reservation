import { Reservation } from "./reservation.model";

export interface Flight {
    id: string;
    number: string;
    departureTime: string; // ISO string, np. "2025-04-12T10:00:00Z"
    arrivalTime: string; // ISO string, np. "2025-04-12T10:00:00Z"
    reservations?: Reservation[]; // w celu łatwiejszego wyświetlenia
    reservationPassengers?: string; // w celu łatwiejszego przeszukiwania
    reservationClass?: string;  // w celu łatwiejszego przeszukiwania
}
