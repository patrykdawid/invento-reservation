export interface Reservation {
    id: string;
    passengerName: string;
    flightId: string;
    class: TicketClass;
}

export enum TicketClass {
    Economy = 0,
    Business = 1,
}

export const TICKET_CLASS_LABELS: Record<TicketClass, string> = {
    [TicketClass.Economy]: 'Ekonomiczna',
    [TicketClass.Business]: 'Biznesowa',
};
    
export const TICKET_CLASS_SEVERITY: Record<TicketClass, 'info' | 'warn'> = {
    [TicketClass.Economy]: 'info',
    [TicketClass.Business]: 'warn',
};
