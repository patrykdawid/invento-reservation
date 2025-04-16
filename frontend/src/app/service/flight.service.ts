import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Flight } from '../model/flight.model';

@Injectable({
  providedIn: 'root'
})
export class FlightService {
  constructor(private http: HttpClient) {}

  createFlight(flight: Flight): Observable<Flight> {
      return this.http.post<Flight>('/api/flights', flight);
  }

  updateFlight(flight: Flight): Observable<Flight> {
    return this.http.put<Flight>(`/api/flights/${flight.id}`, flight);
  }
}
