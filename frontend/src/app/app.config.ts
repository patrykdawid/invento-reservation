import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import localePl from '@angular/common/locales/pl';
import { ApplicationConfig, LOCALE_ID, provideZoneChangeDetection } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import Material from '@primeng/themes/material';
import Aura from '@primeng/themes/aura';

import { providePrimeNG } from 'primeng/config';
import { routes } from './app.routes';

registerLocaleData(localePl);

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    { provide: LOCALE_ID, useValue: 'pl' },

    provideHttpClient(withInterceptorsFromDi()),
    provideAnimations(),
    provideAnimationsAsync(),

    providePrimeNG({
      theme: {
          preset: Aura,//Material
          options: {
              darkModeSelector: 'light',
          }
      },
      translation: {
        accept: 'Akceptuj',
        reject: 'Odrzuć',
        
        startsWith: 'Zaczyna się od',
        contains: 'Zawiera',
        notContains: 'Nie zawiera',
        endsWith: 'Kończy się na',
        equals: 'Równa się',
        notEquals: 'Nie równa się',
    
        // dla typów numerycznych i dat
        lt: 'Mniejsze niż',
        lte: 'Mniejsze lub równe',
        gt: 'Większe niż',
        gte: 'Większe lub równe',
    
        // dla dat
        dateIs: 'Data to',
        dateIsNot: 'Data to nie',
        dateBefore: 'Data przed',
        dateAfter: 'Data po',
    
        // przyciski i operatory
        clear: 'Wyczyść',
        apply: 'Zastosuj',
        matchAll: 'Wszystkie warunki',
        matchAny: 'Dowolny warunek',
        addRule: 'Dodaj warunek',
        removeRule: 'Usuń warunek',
        noFilter: 'Brak filtra',
    
        // dni i miesiące – do datepicker'a
        dayNames: ['niedziela','poniedziałek','wtorek','środa','czwartek','piątek','sobota'],
        dayNamesShort: ['ndz','pon','wt','śr','czw','pt','sob'],
        dayNamesMin: ['Nd','Pn','Wt','Śr','Cz','Pt','So'],
        monthNames: ['styczeń','luty','marzec','kwiecień','maj','czerwiec','lipiec','sierpień','wrzesień','październik','listopad','grudzień'],
        monthNamesShort: ['sty','lut','mar','kwi','maj','cze','lip','sie','wrz','paź','lis','gru'],
        today: 'Dziś',
        weekHeader: 'Tydzień',
        firstDayOfWeek: 1
      },
  }),
  ],
};
