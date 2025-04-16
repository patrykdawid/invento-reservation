import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { Flight } from '../model/flight.model';

@Component({
  selector: 'app-flight-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputTextModule, DatePickerModule, ButtonModule],
  templateUrl: './flight-form.component.html',
  styleUrl: './flight-form.component.scss',
})
export class FlightFormComponent implements OnInit {
  @Input() flight?: Flight;
  @Output() saveFlight = new EventEmitter<Flight>();

  form!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const departureDate = this.flight?.departureTime ? new Date(this.flight.departureTime) : null;
    const arrivalDate = this.flight?.arrivalTime ? new Date(this.flight.arrivalTime) : null;

    this.form = this.fb.group(
      {
        number: [this.flight?.number, Validators.required],
        departureTime: [departureDate, Validators.required],
        arrivalTime: [arrivalDate, Validators.required],
      },
      { validators: [this.departureBeforeArrivalValidator] }
    );
  }

  departureBeforeArrivalValidator(group: AbstractControl): ValidationErrors | null {
    const departure = group.get('departureTime')?.value;
    const arrival = group.get('arrivalTime')?.value;

    if (departure && arrival && new Date(departure) >= new Date(arrival)) {
      return { invalidTimeRange: true };
    }
    return null;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const result: Flight = {
      id: this.flight?.id,
      number: this.form.get('number')!.value,
      departureTime: this.form.get('departureTime')!.value,
      arrivalTime: this.form.get('arrivalTime')!.value,
      reservations: this.flight?.reservations ?? [],
    };

    this.saveFlight.emit(result);
  }
}
