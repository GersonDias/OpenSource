/// <reference path='../_all.ts' />
/// <reference path="../../typings/tsd.d.ts" />

module todos {
    'use strict';

    export class todoCtrl {
        private todos: todoItem[];
        private workItemId: number;
        

        public static $inject = [
            '$scope',
            '$location',
            'todoStorage',
            'filterFilter'
        ];
        
        constructor(
            private $scope: ITodoScope,
            private $location: ng.ILocationService,
            private todoStorage: ITodoStorage,
            private filterFilter
        ) {
            var self = this;

            VSS.require(['TFS/WorkItemTracking/Services'], function (workItemService) {
                workItemService.WorkItemFormService.getService().then(function (workItem) {
                    workItem.getId().then(function (workItemId) {
                        var todos = todoStorage.get(workItemId);
                        self.todos = todos;

                        self.workItemId = workItemId; 

                        $scope.newTodo = '';
                        $scope.editedTodo = null;

                        $scope.vm = self;
                       
                        $scope.todos = todos;                         
                        
                        $scope.$apply();
                    });
                })
            });          
            
            $scope.$watch('todos', () => self.onTodos(), true);
            $scope.$watch('location.path()', path => self.onPath(path));

            if ($location.path() === '') $location.path('/');
            $scope.location = $location;                      
        }

        
        onPath(path:string){
            this.$scope.statusFilter = (path === '/active') ?
                { completed:false } : (path === '/completed') ? 
                { completed:true} : {};
        }

        onTodos() {
            this.$scope.remainingCount  = this.filterFilter(this.todos, { completed: false }).length;
            this.$scope.doneCount       = this.todos.length - this.$scope.remainingCount;
            this.$scope.allChecked      = !this.$scope.remainingCount;
            this.todoStorage.put(this.todos, this.workItemId);
        }

        addTodo() {
            var newTodo: string = this.$scope.newTodo.trim();
            if (!newTodo.length) { return };

            this.todos.push(new todoItem(newTodo, false, this.workItemId));
            this.$scope.newTodo = '';
        }

        editTodo(todoItem) {
            this.$scope.editedTodo = todoItem;

            this.$scope.originalTodo = angular.extend({}, todoItem);
        }

        revertEdits(todoItem: todoItem){
            this.todos[this.todos.indexOf(todoItem)] = this.$scope.originalTodo;
            this.$scope.reverted = true;
        }

        doneEditing(todoItem: todoItem) {
            this.$scope.editedTodo = null;
            this.$scope.originalTodo = null;
            if (this.$scope.reverted) {
                this.$scope.reverted = null;
                return;
            }

            todoItem.title = todoItem.title.trim();

            if (!todoItem.title)
                this.removeTodo(todoItem);
        }

        removeTodo(todoItem: todoItem) {
            this.todos.splice(this.todos.indexOf(todoItem), 1);
        }

        clearDoneTodos() {
            this.$scope.todos = this.todos = this.todos.filter(todoItem => !todoItem.completed)
        }

        markAll(completed: boolean) {
            this.todos.forEach(todoItem => {
                todoItem.completed = completed;
            });
        }
    }
}