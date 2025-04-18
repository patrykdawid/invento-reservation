import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmationService, FilterMetadata, MessageService, SortEvent } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { Table, TableModule, TableRowExpandEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { Observable, Subject, takeUntil } from 'rxjs';
import { FlightFormComponent } from '../flight-form/flight-form.component';
import { Flight } from "../model/flight.model";
import { Reservation, TICKET_CLASS_LABELS, TICKET_CLASS_SEVERITY, TicketClass } from "../model/reservation.model";
import { ReservationFormComponent } from '../reservation-form/reservation-form.component';
import { FlightService } from '../service/flight.service';
import { ReservationService } from '../service/reservation.service';
import { nameof } from '../utils/nameof';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [
    CommonModule,
    ToolbarModule,
    ButtonModule,
    TableModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    DatePickerModule,
    TagModule,
    DropdownModule,
    FormsModule,
    DialogModule,
    ConfirmDialogModule,
    ToastModule,
    FlightFormComponent,
    ReservationFormComponent,
  ],
  templateUrl: './table.component.html',
  styleUrl: './table.component.scss',
  providers: [MessageService, ConfirmationService, ReservationService],
})
export class TableComponent implements OnInit, OnDestroy {

  @ViewChild('dt', { static: false })
  dt!: Table;

  @ViewChild(FlightFormComponent, { static: false })
  flightForm!: FlightFormComponent;

  @ViewChild(ReservationFormComponent, { static: false })
  reservationForm!: ReservationFormComponent;

  flightDialog: boolean = false;
  flightItem?: Flight = undefined!;

  reservationDialog: boolean = false;
  reservationItem: Reservation | undefined = undefined!;

  pageSize = 50;
  flights: Flight[] = [];
  loading: boolean = true;
  expandedRows: { [flightId: string]: boolean } = {};
  initialValue: Flight[] = [];
  isSorted: boolean | null = null;

  flightIdKey = nameof<Flight>('id');
  flightNumberKey = nameof<Flight>('number');
  flightDepartureTimeKey = nameof<Flight>('departureTime');
  flightArrivalTimeKey = nameof<Flight>('arrivalTime');
  flightReservationPassengersKey = nameof<Flight>('reservationPassengers');
  flightReservationClassKey = nameof<Flight>('reservationClass');

  reservationIdKey = nameof<Reservation>('id');
  reservationPassengerNameKey = nameof<Reservation>('passengerName');
  reservationClassKey = nameof<Reservation>('class');

  dateMatchModes = [
    { label: 'Data to', value: 'dateIs' },
    { label: 'Data przed', value: 'dateBefore' },
    { label: 'Data po', value: 'dateAfter' },
    { label: 'Zakres dat', value: 'between' },
  ];
  defaultDateFilterMatchMode: string = this.dateMatchModes[0].value;

  ticketClassOptions = [
    { label: 'Ekonomiczna', value: TicketClass.Economy },
    { label: 'Biznesowa', value: TicketClass.Business }
  ];

  private destroyed$ = new Subject<void>();

  constructor(
      private reservationService: ReservationService,
      private flightService: FlightService,
      private messageService: MessageService,
      private confirmationService: ConfirmationService,
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  loadData() {
    this.reservationService.getFlightsWithReservations()
      .pipe(takeUntil(this.destroyed$))
      .subscribe({
        next: (data : Flight[]) => {
          this.initialValue = [...data];
          this.flights = data.map(f => (<Flight>{
            ...f,
            reservationPassengers: f.reservations?.map(r => r.passengerName).join(', ') ?? '',
            reservationClasses: Array.from(new Set(f.reservations?.map(r => r.class))).join(', ') ?? ''
          }));

          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  trackByFlight(index: number, item: Flight): string {
    return item.id!;
  }

  trackByReservation(index: number, reservation: Reservation): string {
    return reservation.id;
  }

  customSort(event: SortEvent) {
      if (this.isSorted == null || this.isSorted === undefined) {
          this.isSorted = true;
          this.sortTableData(event);
      } else if (this.isSorted == true) {
          this.isSorted = false;
          this.sortTableData(event);
      } else if (this.isSorted == false) {
          this.isSorted = null;
          this.flights = [...this.initialValue];
          this.dt?.reset();
      }
  }

  applyCustomDateFilter(value: Date | Date[], field: string) {
    const filters = this.dt.filters[field];
    let filter: FilterMetadata;

    if (filters instanceof Array){
      if(filters.length === 1) {
        filter = filters[0];
      } else {
        throw new Error('Invalid filter array length. Expected 1, but got ' + filters.length);
      }
    } else {
      filter = filters;
    }

    if (!value || !field) {
      return;
    }

    const matchMode = filter.matchMode;
  
    switch (matchMode) {
      case 'dateIs':
      case 'dateBefore':
      case 'dateAfter':
        this.dt.filter(value, field, matchMode);
        break;
  
      case 'between':
        if (Array.isArray(value) && value.length === 2) {
          this.dt.filter(value, field, matchMode);
        }
        break;
  
      default:
        this.dt.filter(value, field, matchMode ?? 'equals');
        break;
    }
  }

  getDatePickerMode(field: string): 'range' | 'single' {
    const filters = this.dt.filters[field];
    let filter: FilterMetadata;

    if (filters instanceof Array){
      if (filters.length === 1) {
        filter = filters[0];
      } else {
        throw new Error('Invalid filter array length. Expected 1, but got ' + filters.length);
      }
    } else {
      filter = filters;
    }

    if (filter.matchMode === 'between') {
      return 'range';
    }
    return 'single';
  } 

  onRowExpand(event: TableRowExpandEvent) {
    this.expandedRows[event.data[this.flightIdKey]] = true;
  }

  onRowCollapse(event: TableRowExpandEvent) {
    delete this.expandedRows[event.data[this.flightIdKey]];
  }

  getTicketClassLabel(classValue: string): string {
    return TICKET_CLASS_LABELS[classValue as TicketClass] ?? 'Nieznana';
  }

  getTicketClassSeverity(classValue: string): 'info' | 'warn' | 'secondary' {
    return TICKET_CLASS_SEVERITY[classValue as TicketClass] ?? 'secondary';
  }

  showFlightDialog(flight?: Flight) {
    this.flightItem = flight;
    this.flightDialog = true;
  }

  hideFlightDialog() {
    this.flightDialog = false;
    this.flightItem = undefined!;
  }

  onFlightSaved(flight: Flight): void {
    const isExisting = flight.id !== undefined && flight.id !== null;
  
    const request$: Observable<Flight> = isExisting
    ? this.flightService.updateFlight(flight)
    : this.flightService.createFlight(flight);

    request$
      .pipe(takeUntil(this.destroyed$))
      .subscribe({
        next: (res) => {
          if (isExisting) {
            this.updateFlightLocally(res);
            this.messageService.add({
              severity: 'success',
              summary: 'Zaktualizowano lot',
              detail: `Numer: ${res.number}`,
            });
          } else {
            this.addFlightLocally(res);
            this.messageService.add({
              severity: 'success',
              summary: 'Dodano lot',
              detail: `Numer: ${res.number}`,
            });
          }
      
          this.flightDialog = false;
        },
        error: err => {
          this.handleServerValidationError(err);
        }
      });
  }

  showReservationDialog(reservation?: Reservation, flight?: Flight) {
    if (reservation) {
        this.reservationItem = reservation;
    } else if (flight){
        this.reservationItem = <Reservation>{
            flightId: flight.id,
        };
    } else {
      throw new Error('Reservation or flight must be provided to show reservation dialog.');
    }
    this.reservationDialog = true;
  }

  hideReservationDialog() {
    this.reservationDialog = false;
  }

  onReservationSaved(reservation: Reservation): void {
    const isExisting = reservation.id !== undefined && reservation.id !== null;
  
    const request$: Observable<Reservation> = isExisting
    ? this.reservationService.updateReservation(reservation)
    : this.reservationService.createReservation(reservation);
  
    request$
      .pipe(takeUntil(this.destroyed$))
      .subscribe({
        next: (res) => {
          if (isExisting) {
            this.updateReservationLocally(res);
            this.messageService.add({
              severity: 'success',
              summary: 'Zaktualizowano rezerwację',
              detail: `Pasażer: ${res.passengerName}`,
            });
          } else {
            this.addReservationLocally(res);
            this.messageService.add({
              severity: 'success',
              summary: 'Dodano rezerwację',
              detail: `Pasażer: ${res.passengerName}`,
            });
          }
      
          this.reservationDialog = false;
        },
        error: err => {
          this.handleServerValidationError(err);
        }
      });
  }

  deleteReservation(reservation: Reservation) {
      this.confirmationService.confirm({
          message: 'Na pewno usunąć tę rezerwację?',
          header: 'Wymagane potwierdzenie',
          icon: 'pi pi-exclamation-triangle',
          accept: () => {
            this.reservationService.delete(reservation.id)
              .pipe(takeUntil(this.destroyed$))
              .subscribe({
                next: () => {
                  this.removeReservationLocally(reservation);

                  this.messageService.add({
                      severity: 'success',
                      summary: 'Sukces',
                      detail: 'Rezerwacja usunięta',
                      life: 3000,
                  });

                },
                error: err => {
                  this.handleServerValidationError(err);
                }
              });
          },
      });
  }

  private removeReservationLocally(reservation: Reservation): void {
    const flight = this.flights.find(f => f.id === reservation.flightId);
    if (!flight) return;
  
    flight.reservations = flight.reservations!.filter(r => r.id !== reservation.id);
  }

  private updateReservationLocally(reservation: Reservation): void {
    const flight = this.flights.find(f => f.id === reservation.flightId);
    if (!flight) return;

    flight.reservations = flight.reservations!.map(r =>
      r.id === reservation.id ? { ...reservation } : r
    );
  }

  private addReservationLocally(reservation: Reservation): void {
    const flight = this.flights.find(f => f.id === reservation.flightId);
    if (!flight) return;
  
    flight.reservations = [...(flight.reservations ?? []), reservation];
  }

  private updateFlightLocally(updatedFlight: Flight): void {
    const flight = this.flights.find(f => f.id === updatedFlight.id);
    if (!flight) return;
  
    flight.number = updatedFlight.number;
    flight.departureTime = updatedFlight.departureTime;
    flight.arrivalTime = updatedFlight.arrivalTime;
  }

  private addFlightLocally(flight: Flight): void {
    this.flights.push(flight);

    const sortField = this.dt.sortField;
    const sortOrder = this.dt.sortOrder;

    if (sortField && sortOrder) {
      this.sortTableData({
        data: this.flights,
        field: sortField,
        order: sortOrder
      });
    }
  }

  private sortTableData(event: SortEvent) {
    const data = event.data as Flight[];
    const field = event.field as keyof Flight;
    const order = event.order;
  
    if (!data || !field || !order) {
      return;
    }
  
    data.sort((a, b) => {
      const v1 = a[field];
      const v2 = b[field];
  
      if (v1 == null && v2 != null) return -1 * order;
      if (v1 != null && v2 == null) return 1 * order;
      if (v1 == null && v2 == null) return 0;
  
      if (typeof v1 === 'string' && typeof v2 === 'string') {
        return v1.localeCompare(v2) * order;
      }
  
      return (v1! < v2! ? -1 : v1! > v2! ? 1 : 0) * order;
    });
  }

  private handleServerValidationError(error: any): void {
    if (error.status === 400 && error.error?.errors) {
      const messages = Object.values(error.error.errors).flat();
  
      for (const message of messages) {
        this.messageService.add({
          severity: 'error',
          summary: 'Validation error',
          detail: message as string,
        });
      }
    } else if (error.status === 500 && error.error?.detail?.includes('Error ID')) {
      this.messageService.add({
        severity: 'error',
        summary: 'Unexpected server error',
        detail: `An unexpected error occurred. ${error.error.detail}`
      });
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Server error',
        detail: 'Unexpected error occurred. Please try again later.'
      });
    }
  }
}
