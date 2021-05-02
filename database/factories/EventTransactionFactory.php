<?php

namespace Database\Factories;

use App\Models\EventTransaction;
use App\Models\Event;
use App\Models\Transaction;
use Illuminate\Database\Eloquent\Factories\Factory;

class EventTransactionFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = EventTransaction::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
           'event_id'=>Event::factory(),
           'transaction_id'=>Transaction::factory()
        ];
    }
}
