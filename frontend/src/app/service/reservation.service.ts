import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Flight } from "../model/flight.model";
import { PagedResult } from "../model/paged-result.model";

@Injectable()
export class ReservationService {
    constructor(private http: HttpClient) {}

    getFlightsWithReservations(page: number, size: number): Observable<PagedResult<Flight>> {
        return this.http.get<PagedResult<Flight>>(`/api/flights?pageNumber=${page}&pageSize=${size}&withReservations=true`);
    }
}
