import { Reservation } from "./reservation.model";

export interface Flight {
    id: string;
    number: string;
    departureTime: string; // ISO string, np. "2025-04-12T10:00:00Z"
    arrivalTime: string;
    reservations?: Reservation[];
    reservationPassengers?: string;
    reservationClass?: string;
}
