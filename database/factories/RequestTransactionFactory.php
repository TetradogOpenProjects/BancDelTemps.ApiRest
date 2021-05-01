<?php

namespace Database\Factories;

use App\Models\RequestTransaction;
use Illuminate\Database\Eloquent\Factories\Factory;

class RequestTransactionFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = RequestTransaction::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
            'request_id'=>Request::factory(),
            'transaction_id'=>Transaction::factory()
        ];
    }
}
