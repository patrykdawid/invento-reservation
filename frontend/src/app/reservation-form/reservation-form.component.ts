import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';

export enum TicketClass {
  Economy = 0,
  Business = 1
}

export interface Reservation {
  id: string;
  flightId: string;
  passengerName: string;
  class: TicketClass;
}

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputTextModule, SelectModule, ButtonModule],
  templateUrl: './reservation-form.component.html',
})
export class ReservationFormComponent implements OnInit {
  @Input() reservation!: Reservation;
  @Output() saveReservation = new EventEmitter<Reservation>();

  form!: FormGroup;

  ticketClassOptions = [
    { label: 'Ekonomiczna', value: TicketClass.Economy },
    { label: 'Biznesowa', value: TicketClass.Business }
  ];

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      passengerName: [this.reservation?.passengerName ?? '', Validators.required],
      class: [this.reservation?.class ?? null, Validators.required]
    });
  }

  save(): void {
    if (this.form.valid) {
      const updated: Reservation = {
        ...this.reservation,
        ...this.form.value,
      };

      this.saveReservation.emit(updated);
    } else {
      this.form.markAllAsTouched();
    }
  }
}
