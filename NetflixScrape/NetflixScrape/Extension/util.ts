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
    export class FutureThing<T> {
        constructor(initialValue?: T) {
            if (initialValue) {
                this.promise = Promise.resolve(initialValue);
                this.resolve = () => { };
                this.reject = () => { };
            } else {
                this.reset();
            }
        }
        private promise: Promise<T> = null;
        private resolve: (value?: ResolveArg<T>) => void = null;
        private reject: (reason?: any) => void = null;
        public get(): Promise<T> { return this.promise; }
        public reset() {
            this.promise = new Promise<T>((resolve, reject) => {
                this.resolve = resolve;
                this.reject = reject;
            });
            //this.promise.catch().then(() => {
            //    this.resolve = null;
            //    this.reject = null;
            //})
        }
        public set(value: T) {
            this.resolve(value);
        }
        public error(reason?: any) {
            this.error(reason);
        }
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