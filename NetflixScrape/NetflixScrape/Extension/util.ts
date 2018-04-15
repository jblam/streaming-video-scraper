namespace JBlam.NetflixScrape.Extension.Util {

    type ResolveArg<TResult> = TResult | PromiseLike<TResult>

    export class PromiseResolver<TResult> {
        private readonly map = new Map<number, { resolve: (value?: ResolveArg<TResult>) => void, reject: (reason?: any) => void }>();
        // do not start at zero, to avoid attempting to resolve/reject default value (or coercions thereof)
        private latestKey: number = 1;
        public enregister() {
            let i = this.latestKey;
            this.latestKey += 1;
            var promise = new Promise<TResult>((resolve, reject) => {
                this.map.set(i, { resolve, reject });
            });
            promise.catch().then(() => this.map.delete(i));
            return { key: i, promise };
        }
        public resolve(key: number, value?: ResolveArg<TResult>) {
            if (!this.map.has(key)) { throw new Error("Cannot find promise with given key"); }
            this.map.get(key).resolve(value);
        }
        public reject(key: number, reason?: Error) {
            if (!this.map.has(key)) { throw new Error("Cannot find promise with given key"); }
            this.map.get(key).reject(reason);
        }
    }
    export class Future<T> implements PromiseLike<T>{
        /**
         * Attaches callbacks for the resolution and/or rejection of the Promise.
         * @param onfulfilled The callback to execute when the Promise is resolved.
         * @param onrejected The callback to execute when the Promise is rejected.
         * @returns A Promise for the completion of which ever callback is executed.
         */
        then<TResult1 = T, TResult2 = never>(onfulfilled?: (value: T) => ResolveArg<TResult1>, onrejected?: (reason: any) => ResolveArg<TResult2>): PromiseLike<TResult1 | TResult2> {
            return this.promise.then(onfulfilled, onrejected);
        }
        /**
         * Gets the current value of the Future, if resolved; throws if rejected; or returns null if the Future is still pending
         */
        value(): T {
            if (this.currentError) {
                throw this.currentError.value;
            }
            return this.currentValue.value;
        }
        /**
         * Ensures the Future returns the supplied value.
         *
         * If the current state is pending, the existing promise will be resolved with the value.
         * If the current state is resolved, the state of the Future will be reset to the supplied value.
         * @param value the value with which the Future should resolve
         */
        set(value: T) {
            if (this.resolve) { this.resolve(value); }
            else {
                this.setPromise(Promise.resolve(value));
            }
        }
        /**
         * Ensures that the Future rejects with the supplied error.
         *
         * If the Future is pending, the existing promise will reject with the error.
         * If the Future is resolved, the state of the Future will be reset to reject with the supplied error.
         * @param err the error to reject
         */
        error(err: any) {
            if (this.reject) { this.reject(err); }
            else {
                this.setPromise(Promise.reject(err));
            }
        }
        /**
         * Resets the current state of the Future to be pending
         */
        reset() {
            if (this.reject) { this.reject("Future was reset"); }
            this.currentValue = null;
            this.currentError = null;
            this.setPromise(new Promise<T>((res, rej) => {
                this.resolve = res;
                this.reject = rej;
            }));
        }
        /**
         * Gets the current state of the Future
         */
        get state(): "pending" | "fulfilled" | "rejected" | null {
            if (this.currentValue) { return "fulfilled"; }
            if (this.currentError) { return "rejected"; }
            if (this.promise) { return "pending"; }
            return null;
        }

        private setPromise(newPromise: Promise<T>) {
            this.promise = newPromise;
            newPromise.then(val => this.onResolve(val), err => this.onResolve(err)).then(() => this.onCompletion());
        }
        private onReject(err: any) {
            this.currentValue = null;
            this.currentError = { value: err };
        }
        private onResolve(value: T) {
            this.currentValue = { value };
            this.currentError = null;
        }
        private onCompletion() {
            this.resolve = null;
            this.reject = null;
        }
        private promise: Promise<T>;
        private currentValue: { value: T } | null;
        private currentError: { value: any } | null;
        private resolve: (value: ResolveArg<T>) => void;
        private reject: (reason?: any) => void;
    }

    export interface IEvent<TMessage> {
        addListener(listener: (msg: TMessage) => void): void;
        removeListener(listener: (msg: TMessage) => void): void;
        name: string;
    }
    export class Event<TMessage> implements IEvent<TMessage> {
        constructor(name: string) {
            this.name = name;
        }
        public readonly name: string;
        private readonly listeners = new Set<(msg: TMessage) => void>();
        raise(msg: TMessage) {
            if (!this.listeners) { debugger; }
            for (let l of this.listeners) {
                l(msg);
            }
        }
        reset() { this.listeners.clear(); }
        addListener(listener: (msg: TMessage) => void): void {
            this.listeners.add(listener);
        }
        removeListener(listener: (msg: TMessage) => void) {
            this.listeners.delete(listener);
        }
    }
}