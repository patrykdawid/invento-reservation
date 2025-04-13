import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Flight } from "../model/flight.model";

@Injectable()
export class ReservationService {
    constructor(private http: HttpClient) {}

    getFlightsWithReservations(): Observable<Flight[]> {
        return this.http.get<Flight[]>(`/api/flights?withReservations=true`);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`/api/reservations/${id}`);
    }
}
