import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Flight } from "../model/flight.model";
import { Reservation } from "../model/reservation.model";

@Injectable()
export class ReservationService {
    constructor(private http: HttpClient) {}

    getFlightsWithReservations(): Observable<Flight[]> {
        return this.http.get<Flight[]>(`/api/flights?withReservations=true`);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`/api/reservations/${id}`);
    }

    createReservation(reservation: Reservation): Observable<Reservation> {
        return this.http.post<Reservation>('/api/reservations', reservation);
    }

    updateReservation(reservation: Reservation): Observable<Reservation> {
        return this.http.put<Reservation>(`/api/reservations/${reservation.id}`, reservation);
    }
}
