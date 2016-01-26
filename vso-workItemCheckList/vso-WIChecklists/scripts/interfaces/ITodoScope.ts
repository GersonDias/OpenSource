/// <reference path='../_all.ts' />

module todos {
    export interface ITodoScope extends ng.IScope {
        todos: todoItem[];
        newTodo: string;
        editedTodo: todoItem;
        originalTodo: todoItem;
        remainingCount: number;
        doneCount: number;
        allChecked: boolean;
        reverted: boolean;
        statusFilter: { completed?: boolean};
        location: ng.ILocationService;
        vm: todoCtrl;
    }
}